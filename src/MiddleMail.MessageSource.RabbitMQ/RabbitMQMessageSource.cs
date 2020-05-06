using System;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Scheduling;
using MiaPlaza.MiddleMail.Model;
using Microsoft.Extensions.Logging;

namespace MiaPlaza.MiddleMail.MessageSource.RabbitMQ {
	public class RabbitMQMessageSource : IMessageSource, IDisposable {

		private readonly IBus bus;
		private ISubscriptionResult subscriptionResult;
		private readonly IRetryDelayStrategy retryDelayStrategy;
		private readonly ILogger<RabbitMQMessageSource> logger;

		private readonly RabbitMQMessageSourceConfiguration configuration;

		public RabbitMQMessageSource(RabbitMQMessageSourceConfiguration configuration, IRetryDelayStrategy retryDelayStrategy, ILogger<RabbitMQMessageSource> logger) {
			this.configuration = configuration;
			this.bus = RabbitHutch.CreateBus(configuration.ConnectionString, x => x.Register<IScheduler, DelayedExchangeScheduler>());
			this.retryDelayStrategy = retryDelayStrategy;
			this.logger = logger;
		}

		public void Start(Func<EmailMessage, Task> callback) {
			subscriptionResult = bus.SubscribeAsync<EmailMessage>(configuration.SubscriptionId, callback);
		}

		public void Stop() {
			// disposing the subscription result stops our subscription and we do not accept new messages
			subscriptionResult?.Dispose();
		}

		public async Task RetryAsync(EmailMessage emailMessage) {
			emailMessage.RetryCount++;
			var delay = retryDelayStrategy.GetDelay(emailMessage.RetryCount);
			await bus.FuturePublishAsync(DateTime.UtcNow.AddSeconds(delay), emailMessage);
			logger.LogError($"Delaying {emailMessage.Id} for {new TimeSpan(0, 0, delay)}");
		}

		public void Dispose() {
			bus?.Dispose();
		}
	}
}
