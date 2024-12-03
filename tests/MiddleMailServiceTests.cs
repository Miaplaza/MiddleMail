using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Management.Client;
using EasyNetQ.Scheduling;
using MiddleMail.Exceptions;
using MiddleMail.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Microsoft.Extensions.Options;

namespace MiddleMail.Tests {

	public class MiddleMailServiceTests : IDisposable {

		private const string SINGLE_PROCESSING_EXCEPTION = "SINGLE_PROCESSING_EXCEPTION";
		private const string GENERAL_PROCESSING_EXCEPTION = "GENERAL_PROCESSING_EXCEPTION";
		private const string EVERYTHING_FINE = "EVERYTHING_FINE";
		private const int RATE_LIMIT_PER_MINUTE = 3;

		private static readonly TimeSpan delayBeforeCheckingIfEmailSent = TimeSpan.FromSeconds(5);

		private readonly Mock<IMessageProcessor> processorMock;
		private readonly Mock<IMessageSource> messageSourceMock;
		private readonly MiddleMailService mailService;
		private Func<EmailMessage, Task> callback;

		
		private readonly Mock<IMessageProcessor> rateLimitedProcessorMock;
		private readonly Mock<IMessageSource> rateLimitedMessageSourceMock;
		private readonly MiddleMailService rateLimitedService;
		private Func<EmailMessage, Task> rateLimitedCallback;

		public MiddleMailServiceTests() {
			processorMock = new Mock<IMessageProcessor>();
			processorMock
				.Setup(m => m.ProcessAsync(It.Is<EmailMessage>(m => m.Subject != GENERAL_PROCESSING_EXCEPTION || m.Subject != SINGLE_PROCESSING_EXCEPTION)))
				.Returns(async(EmailMessage emailMessage) => {
					await Task.Delay(1000);
				});

			processorMock
				.Setup(m => m.ProcessAsync(It.Is<EmailMessage>(m => m.Subject == SINGLE_PROCESSING_EXCEPTION)))
				.ThrowsAsync(new SingleProcessingException());

			processorMock
				.Setup(m => m.ProcessAsync(It.Is<EmailMessage>(m => m.Subject == GENERAL_PROCESSING_EXCEPTION)))
				.ThrowsAsync(new GeneralProcessingException());

			messageSourceMock = new Mock<IMessageSource>();
			messageSourceMock
				.Setup(m => m.RetryAsync(It.IsAny<EmailMessage>()));

			messageSourceMock
				.Setup(m => m.Start(It.IsAny<Func<EmailMessage, Task>>()))
				.Callback((Func<EmailMessage, Task> callback) => {
					this.callback = callback;
				});
				
			messageSourceMock
				.Setup(m => m.Stop());

			var logger = new NullLogger<MiddleMailService>();
			
			var options = Options.Create(new MiddleMailOptions { RateLimited = false });
			mailService = new MiddleMailService(options, processorMock.Object, logger, messageSourceMock.Object);

			rateLimitedProcessorMock = new Mock<IMessageProcessor>();
			rateLimitedProcessorMock
				.Setup(m => m.ProcessAsync(It.Is<EmailMessage>(m => true)))
				.Returns(async(EmailMessage emailMessage) => {
					await Task.Delay(1000);
				});

			rateLimitedMessageSourceMock = new Mock<IMessageSource>();
			rateLimitedMessageSourceMock
				.Setup(m => m.RetryAsync(It.IsAny<EmailMessage>()));

			rateLimitedMessageSourceMock
				.Setup(m => m.Start(It.IsAny<Func<EmailMessage, Task>>()))
				.Callback((Func<EmailMessage, Task> callback) => {
					this.rateLimitedCallback = callback;
				});
				
			rateLimitedMessageSourceMock
				.Setup(m => m.Stop());

			var rateLimitedOptions = Options.Create(new MiddleMailOptions { RateLimited = true, LimitPerMinute = RATE_LIMIT_PER_MINUTE });
			rateLimitedService = new MiddleMailService(rateLimitedOptions, rateLimitedProcessorMock.Object, logger, rateLimitedMessageSourceMock.Object);

			Task.WhenAll(mailService.StartAsync(CancellationToken.None), rateLimitedService.StartAsync(CancellationToken.None)).Wait();
		}		

		[Fact]
		public async Task AllConsumerTaskAwaited() {
			var tasks = Enumerable.Range(0, 50).Select(i => callback(FakerFactory.EmailMessageFaker.Generate())).ToArray();
			await mailService.StopAsync(CancellationToken.None);

			Assert.All(tasks, t => Assert.True(t.IsCompletedSuccessfully));
		}

		[Fact]
		public async Task SingleProcessingExceptionBubblesUp() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Subject = SINGLE_PROCESSING_EXCEPTION;
			await Assert.ThrowsAnyAsync<SingleProcessingException>(async () => await callback(emailMessage));
			messageSourceMock.Verify(m => m.RetryAsync(It.IsAny<EmailMessage>()), Times.Never);
		}

		[Fact]
		public async Task GeneralProcessingExceptionCallsRetry() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Subject = GENERAL_PROCESSING_EXCEPTION;
			await callback(emailMessage);
			messageSourceMock.Verify(m => m.RetryAsync(It.IsAny<EmailMessage>()), Times.Once);
		}

		[Fact]
		public async Task ProcessingContinuesAfterSingleProcessingException() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Subject = SINGLE_PROCESSING_EXCEPTION;
			try { await callback(emailMessage);	} catch { }

			for(int i = 1; i <= 10; i++) {
				await callback(FakerFactory.EmailMessageFaker.Generate());
				processorMock.Verify(m => m.ProcessAsync(It.IsAny<EmailMessage>()), Times.Exactly(i + 1));
			}
		}

		[Fact]
		public async Task ProcessingContinuesAfterGeneralProcessingException() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Subject = GENERAL_PROCESSING_EXCEPTION;
			try { await callback(emailMessage);	} catch { }

			for(int i = 1; i <= 10; i++) {
				await callback(FakerFactory.EmailMessageFaker.Generate());
				processorMock.Verify(m => m.ProcessAsync(It.IsAny<EmailMessage>()), Times.Exactly(i + 1));
			}
		}

		[Fact]
		public async Task RateLimitAllowsCorrectNumberThrough() {
			// The rate limit is 3 messages.
			// Start by sending the 3 messages, and verify that they arrive
			for (int i = 0; i < 3; i++) {
				var emailMessage = FakerFactory.EmailMessageFaker.Generate();
				await rateLimitedCallback(emailMessage);
				rateLimitedProcessorMock.Verify(m => m.ProcessAsync(It.IsAny<EmailMessage>()), Times.Exactly(i + 1));
			}

			// Then, attempt to send a 4th message,
			// And verify that it doesn't send.
			var notSentMessage = FakerFactory.EmailMessageFaker.Generate();
			await Task.WhenAny(rateLimitedCallback(notSentMessage), Task.Delay(delayBeforeCheckingIfEmailSent));

			// NOTE: if this assertion fails, it's because you changed the rate limit window.
			// If you do, change the delayBeforeCheckingIfEmailSent accordingly.
			Assert.True(delayBeforeCheckingIfEmailSent < MiddleMailService.RateLimitWindow);
			rateLimitedProcessorMock.Verify(m => m.ProcessAsync(It.IsAny<EmailMessage>()), Times.Exactly(3)); //Not sent
		}

		[Fact]
		public async Task RateLimitAllowsCorrectNumberThroughAfterCorrectDelay() {
			// The rate limit is 3 messages.
			// Start by sending the 3 messages, and verify that they arrive
			for (int i = 0; i < 3; i++) {
				var emailMessage = FakerFactory.EmailMessageFaker.Generate();
				await rateLimitedCallback(emailMessage);
				rateLimitedProcessorMock.Verify(m => m.ProcessAsync(It.IsAny<EmailMessage>()), Times.Exactly(i + 1));
			}

			// Then, attempt to send a 4th message
			DateTime before = DateTime.Now;
			var lastMessage = FakerFactory.EmailMessageFaker.Generate();
			await rateLimitedCallback(lastMessage);

			//Now that the await has completed, verify that it took the right amount of time, but was then sent.
			DateTime after = DateTime.Now;
			Assert.True(after - before > MiddleMailService.RateLimitWindow);
			rateLimitedProcessorMock.Verify(m => m.ProcessAsync(It.IsAny<EmailMessage>()), Times.Exactly(4));
		}

		public void Dispose() {
			mailService.Dispose();
		}


	}
}
