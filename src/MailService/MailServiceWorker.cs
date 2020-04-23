using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using MiaPlaza.MailService.Delivery;
using Microsoft.Extensions.Hosting;

namespace MiaPlaza.MailService {
	internal class MailServiceWorker : IHostedService {
		
		private readonly IBus bus;
		private ISubscriptionResult subscriptionResult;
		private IMessageProcessor processor;

		public MailServiceWorker(IBus bus, IMessageProcessor processor) {
			this.bus = bus;
			this.processor = processor;
		}

		public Task StartAsync(CancellationToken cancellationToken) {
			subscriptionResult = bus.SubscribeAsync<EmailMessage>("mailqueue1", (emailData) => processor.HandleAsync(emailData));
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken) {
			subscriptionResult?.Dispose();
			return Task.CompletedTask;
		}
	}
}
