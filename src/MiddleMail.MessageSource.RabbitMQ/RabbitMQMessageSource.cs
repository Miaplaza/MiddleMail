using System;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Scheduling;
using MiaPlaza.MiddleMail.Model;
using Microsoft.Extensions.Logging;

namespace MiaPlaza.MiddleMail.MessageSource.RabbitMQ {

	/// <summary>
	/// A message source that consumes message from RabbitMQ.
	/// </summary>
	/// <remarks>
	/// Requires the delayed message plugin to be installed in RabbitMQ: https://github.com/rabbitmq/rabbitmq-delayed-message-exchange
	/// which is used for retrying messages.
	/// </remarks>
	public class RabbitMQMessageSource : IMessageSource, IDisposable {

		private readonly IBus bus;
		private ISubscriptionResult subscriptionResult;
		private readonly IBackoffStrategy retryDelayStrategy;
		private readonly ILogger<RabbitMQMessageSource> logger;

		private readonly RabbitMQMessageSourceConfiguration configuration;

		public RabbitMQMessageSource(RabbitMQMessageSourceConfiguration configuration, IBackoffStrategy retryDelayStrategy, ILogger<RabbitMQMessageSource> logger) {
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

			// see https://github.com/EasyNetQ/EasyNetQ/wiki/Support-for-Delayed-Messages-Plugin
			await bus.FuturePublishAsync(DateTime.UtcNow.Add(delay), emailMessage);

			logger.LogError($"Delaying {emailMessage.Id} for {delay}");
		}

		public void Dispose() {
			bus?.Dispose();
		}
	}
}
