using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Management.Client;
using EasyNetQ.Scheduling;
using MiaPlaza.MiddleMail.MessageSource.RabbitMQ;
using MiaPlaza.MiddleMail.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MiaPlaza.MiddleMail.Tests.MessageSource {

	public class RabbitMQMessageSourceTests {
		private const string VHOST_NAME = "test";
		private readonly IBus bus;
		private readonly Mock<IMessageProcessor> callbackMock;
		private readonly RabbitMQMessageSource messageSource;

		private string rabbitMQHost = Environment.GetEnvironmentVariable("RabbitMQ__Host") ?? "localhost";
		
		public RabbitMQMessageSourceTests() {
			setupVhost();
			bus = EasyNetQ.RabbitHutch.CreateBus($"host={rabbitMQHost};virtualHost={VHOST_NAME};prefetchcount=10", x => x.Register<IScheduler, DelayedExchangeScheduler>());

			var retryDelayMock = new Mock<IRetryDelayStrategy>();
			retryDelayMock
				.Setup(m => m.GetDelay(It.IsAny<int>()))
				.Returns(1);

			// We do not need an IMessageProcessor here, but we use it to mock a callback function
			callbackMock = new Mock<IMessageProcessor>();
			callbackMock
				.Setup(m => m.ProcessAsync(It.IsAny<EmailMessage>()));

			var logger = new NullLogger<RabbitMQMessageSource>();
			messageSource = new RabbitMQMessageSource(bus, retryDelayMock.Object, logger);

			messageSource.Start(callbackMock.Object.ProcessAsync);
		}

		/// <summary>
		/// Tests run in a virtualhost https://www.rabbitmq.com/vhosts.html
		/// If the virtualhost is present, we delete it and recreate it before running any test
		/// </summary>
		private void setupVhost() {
			var managementClient = new ManagementClient($"http://{rabbitMQHost}", "guest", "guest", 15672);
			
			var vhost = managementClient.GetVhosts().Where(v => v.Name == VHOST_NAME).FirstOrDefault();
			if (vhost != null) {
				managementClient.DeleteVhost(vhost);
			}
			managementClient.CreateVhost(VHOST_NAME);
		}

		[Fact]
		public async Task NoMessageReceivedAfterStop() {
			messageSource.Stop();
			await bus.PublishAsync(FakerFactory.EmailMessageFaker.Generate());
			await Task.Delay(50);
			callbackMock.Verify(c => c.ProcessAsync(It.IsAny<EmailMessage>()), Times.Never);
		}

		[Fact]
		public async Task MessageReceivedAfterStart() {
			await bus.PublishAsync(FakerFactory.EmailMessageFaker.Generate());
			await Task.Delay(50);
			callbackMock.Verify(c => c.ProcessAsync(It.IsAny<EmailMessage>()), Times.Once);
		}

		[Fact]
		public async Task OneHundredMessageReceivedAfterStart() {
			for(int i = 1; i <= 100; i++) {
				await bus.PublishAsync(FakerFactory.EmailMessageFaker.Generate());
			}
			await Task.Delay(100);
			callbackMock.Verify(c => c.ProcessAsync(It.IsAny<EmailMessage>()), Times.Exactly(100));
		}

		[Fact]
		public async Task RetryRequeuesMessage() {
			await messageSource.RetryAsync(FakerFactory.EmailMessageFaker.Generate());
			await Task.Delay(1050);
			callbackMock.Verify(c => c.ProcessAsync(It.IsAny<EmailMessage>()), Times.Once);
		}
	}
}
