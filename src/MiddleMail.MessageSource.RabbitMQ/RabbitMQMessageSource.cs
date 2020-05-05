using System;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Scheduling;
using MiaPlaza.MiddleMail.Model;
using Microsoft.Extensions.Logging;

namespace MiaPlaza.MiddleMail.MessageSource.RabbitMQ {
	public class RabbitMQMessageSource : IMessageSource {

		private readonly IBus bus;
		private ISubscriptionResult subscriptionResult;
		private readonly IRetryDelayStrategy retryDelayStrategy;
		private readonly ILogger<RabbitMQMessageSource> logger;

		public RabbitMQMessageSource(IBus bus, IRetryDelayStrategy retryDelayStrategy, ILogger<RabbitMQMessageSource> logger) {
			this.bus = bus;
			this.retryDelayStrategy = retryDelayStrategy;
			this.logger = logger;
		}

		public void Start(Func<EmailMessage, Task> callback) {
			subscriptionResult = bus.SubscribeAsync<EmailMessage>("send", callback);
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
	}
}
