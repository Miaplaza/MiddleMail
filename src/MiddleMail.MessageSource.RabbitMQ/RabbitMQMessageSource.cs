using System;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Scheduling;
using MiddleMail.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MiddleMail.MessageSource.RabbitMQ {

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
		private readonly IBackoffStrategy backoffStrategy;
		private readonly ILogger<RabbitMQMessageSource> logger;

		private readonly RabbitMQMessageSourceOptions options;

		public RabbitMQMessageSource(IOptions<RabbitMQMessageSourceOptions> options, IBackoffStrategy backoffStrategy, ILogger<RabbitMQMessageSource> logger) {
			this.options = options.Value;
			this.bus = RabbitHutch.CreateBus(this.options.ConnectionString, x => x.Register<IScheduler, DelayedExchangeScheduler>());
			this.backoffStrategy = backoffStrategy;
			this.logger = logger;
		}

		public void Start(Func<EmailMessage, Task> callback) {
			subscriptionResult = options.Topic != null
				? bus.SubscribeAsync<EmailMessage>(options.SubscriptionId, callback, emailMessage => emailMessage.WithTopic(options.Topic))
				: bus.SubscribeAsync<EmailMessage>(options.SubscriptionId, callback);
		}

		public void Stop() {
			// disposing the subscription result stops our subscription and we do not accept new messages
			subscriptionResult?.Dispose();
		}

		public async Task RetryAsync(EmailMessage emailMessage) {
			emailMessage.RetryCount++;
			var delay = backoffStrategy.GetDelay(emailMessage.RetryCount);

			// see https://github.com/EasyNetQ/EasyNetQ/wiki/Support-for-Delayed-Messages-Plugin
			await bus.FuturePublishAsync(DateTime.UtcNow.Add(delay), emailMessage);

			logger.LogError($"Delaying {emailMessage.Id} for {delay}");
		}

		public void Dispose() {
			bus?.Dispose();
		}
	}
}
