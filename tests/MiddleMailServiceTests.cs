using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MiddleMail.Exceptions;
using MiddleMail.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Xunit;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using System.Diagnostics.Contracts;

namespace MiddleMail.Tests {

	public sealed class MiddleMailServiceTests : IDisposable {

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
		private Mock<RateLimiter> rateLimiterMock;

		public MiddleMailServiceTests() {
			processorMock = new Mock<IMessageProcessor>();
			processorMock
				.Setup(m => m.ProcessAsync(It.Is<EmailMessage>(m => m.Subject != GENERAL_PROCESSING_EXCEPTION || m.Subject != SINGLE_PROCESSING_EXCEPTION)))
				.Returns(async (EmailMessage emailMessage) => {
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

			var noRateLimitOptions = Options.Create(new RateLimiterOptions { LimitPerMinute = null });
			var noRateLimitAccessor = new RateLimiterAccessor(noRateLimitOptions);
			mailService = new MiddleMailService(noRateLimitAccessor, processorMock.Object, logger, messageSourceMock.Object);

			rateLimitedProcessorMock = new Mock<IMessageProcessor>();
			rateLimitedProcessorMock
				.Setup(m => m.ProcessAsync(It.Is<EmailMessage>(m => true)))
				.Returns(async (EmailMessage emailMessage) => {
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

			rateLimiterMock = new Mock<RateLimiter>();
			var rateLimiterAccessorMock = new Mock<IRateLimiterAccessor>();
			rateLimiterAccessorMock
				.SetupGet(rlam => rlam.RateLimiter)
				.Returns(rateLimiterMock.Object);
			rateLimitedService = new MiddleMailService(rateLimiterAccessorMock.Object, rateLimitedProcessorMock.Object, logger, rateLimitedMessageSourceMock.Object);

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
		public async Task EmailsSentWhenRateLimiterPermits() {
			var lease = new Mock<RateLimitLease>();
			lease
				.SetupGet(l => l.IsAcquired)
				.Returns(true);
			rateLimiterMock.Protected()
				.Setup<RateLimitLease>("AttemptAcquireCore", ItExpr.IsAny<int>())
				.Returns(lease.Object);
			
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			await rateLimitedCallback(emailMessage);

			rateLimitedProcessorMock.Verify(m => m.ProcessAsync(It.IsAny<EmailMessage>()));
		}

		[Fact]
		public async Task EmailsNotSentWhenRateLimiterForbids() {
			var lease = new Mock<RateLimitLease>();
			lease
				.SetupGet(l => l.IsAcquired)
				.Returns(false);
			rateLimiterMock.Protected()
				.Setup<RateLimitLease>("AttemptAcquireCore", ItExpr.IsAny<int>())
				.Returns(lease.Object);

			async ValueTask<RateLimitLease> simulateAwaitingLease() {
				await Task.Delay(2 * delayBeforeCheckingIfEmailSent);
				return lease.Object;
			}
			rateLimiterMock.Protected()
				.Setup<ValueTask<RateLimitLease>>("AcquireAsyncCore", ItExpr.IsAny<int>(), ItExpr.IsAny<CancellationToken>())
				.Returns(simulateAwaitingLease);
			
			var notSentMessage = FakerFactory.EmailMessageFaker.Generate();
			await Task.WhenAny(rateLimitedCallback(notSentMessage), Task.Delay(delayBeforeCheckingIfEmailSent));

			rateLimitedProcessorMock.Verify(m => m.ProcessAsync(It.IsAny<EmailMessage>()), Times.Never);
		}

		public void Dispose() {
			mailService.Dispose();
		}


	}
}
