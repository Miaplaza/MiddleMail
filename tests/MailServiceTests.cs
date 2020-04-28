using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using MiaPlaza.MailService.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MiaPlaza.MailService.Tests {

	public class MailServiceTests : IDisposable {

		private const string DELIVER_FAILURE_SINGLE = "DELIVER_FAILURE_SINGLE";
		private const string DELIVER_FAILURE_GLOBAL = "DELIVER_FAILURE_GLOBAL";
		private readonly Mock<IMessageProcessor> processorMock;
		private readonly MailService mailService;
		private readonly IBus bus;
		private readonly Task mailServiceTask;

		private ISubscriptionResult subscriptionResult;

		private readonly Mock<ISubscriptionResult> subscriptionResultMock;
		private readonly SimpleErrorBackoffStrategyConfiguration backoffStrategyConfiguration;
		private readonly Mock<IErrorBackoffStrategy> backoffStrategyMock;

		public MailServiceTests() {
			bus = EasyNetQ.RabbitHutch.CreateBus("host=localhost");

			subscriptionResultMock = new Mock<ISubscriptionResult>();
			subscriptionResultMock
				.Setup(r => r.Dispose())
				.Callback(() => subscriptionResult?.Dispose());

			var busMock = new Mock<IBus>();
			busMock
				.Setup(b => b.SubscribeAsync<EmailMessage>(It.IsAny<string>(), It.IsAny<Func<EmailMessage, Task>>()))
				.Returns((string subscriptionId, Func<EmailMessage, Task> onMessage) => {
					subscriptionResult = bus.SubscribeAsync<EmailMessage>(subscriptionId, onMessage);
					return subscriptionResultMock.Object;
				});
			busMock
				.Setup(b => b.PublishAsync(It.IsAny<EmailMessage>()))
				.Callback((EmailMessage emailMessage) => bus.PublishAsync(emailMessage));

	
			processorMock = new Mock<IMessageProcessor>();

			processorMock
				.Setup(d => d.ProcessAsync(It.Is<EmailMessage>(m => m.Subject == DELIVER_FAILURE_SINGLE)))
				.Throws<SingleDeliveryException>();

			processorMock
				.Setup(d => d.ProcessAsync(It.Is<EmailMessage>(m => m.Subject == DELIVER_FAILURE_GLOBAL)))
				.Throws<GlobalDeliveryException>();

			var config = new Dictionary<string, string>{
				{"SimpleBackoffStrategy:WaitTime", "2000"},
				{"SimpleBackoffStrategy:Threshold", "10"},

			};

			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(config)
				.Build();

			backoffStrategyConfiguration = new SimpleErrorBackoffStrategyConfiguration(configuration);
			
			backoffStrategyMock = new Mock<IErrorBackoffStrategy>();
			

			mailService = new MailService(busMock.Object, processorMock.Object, new NullLogger<MailService>(), backoffStrategyMock.Object);
			mailService.StartAsync(CancellationToken.None);
		}


		[Fact]
		public async void CancellationSuccessful() {		
			Assert.True(bus.IsConnected);
			await mailService.StopAsync(CancellationToken.None);

			await Task.Delay(1200);
			subscriptionResultMock.Verify(s => s.Dispose(), Times.Once);
		}

		public void Dispose() {
			mailService.Dispose();
		}

		[Fact]
		public async void DequeueSingleEmail() {		
			Assert.True(bus.IsConnected);
			
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			await bus.PublishAsync(emailMessage);
			await Task.Delay(50);
			processorMock.Verify(p => p.ProcessAsync(It.IsAny<EmailMessage>()), Times.Once);
		}

		[Fact]
		public async void Dequeue100Email() {		
			Assert.True(bus.IsConnected);
			
			for(var i = 0; i < 100; i++) {
				var emailMessage = FakerFactory.EmailMessageFaker.Generate();
				await bus.PublishAsync(emailMessage);
			}

			await Task.Delay(100);
			processorMock.Verify(p => p.ProcessAsync(It.IsAny<EmailMessage>()), Times.Exactly(100));
		}

		[Fact]
		public async void NotifyGlobalErrorIsCalled() {
			Assert.True(bus.IsConnected);

			for(int i = 1; i <= 5; i++) {
				var emailMessagFailure = FakerFactory.EmailMessageFaker.Generate();
				emailMessagFailure.Subject = DELIVER_FAILURE_GLOBAL;	
				await bus.PublishAsync(emailMessagFailure);

				await Task.Delay(50);
				backoffStrategyMock.Verify(p => p.NotifyGlobalError(), Times.Exactly(i));
			}
		}

		// [Fact]
		// public async void CanProcessAfterSingleDeliveryFailure() {		
		// 	Assert.True(bus.IsConnected);
			
		// 	var emailMessage = FakerFactory.EmailMessageFaker.Generate();
		// 	await bus.PublishAsync(emailMessage);

		// 	var emailMessagFailure = FakerFactory.EmailMessageFaker.Generate();
		// 	emailMessagFailure.Subject = DELIVER_FAILURE_SINGLE;	
		// 	await bus.PublishAsync(emailMessagFailure);

		// 	var emailMessage2 = FakerFactory.EmailMessageFaker.Generate();
		// 	await bus.PublishAsync(emailMessage2);

		// 	await Task.Delay(100);
		// 	processorMock.Verify(p => p.ProcessAsync(It.IsAny<EmailMessage>()), Times.Exactly(3));
		// }


	}
}
