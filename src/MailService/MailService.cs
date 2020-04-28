using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using MiaPlaza.MailService.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MiaPlaza.MailService {
	public class MailService : BackgroundService {
		
		private readonly IBus bus;
		private ISubscriptionResult subscriptionResult;
		private IMessageProcessor processor;
		private readonly ILogger<MailService> logger;
		//private readonly IConfiguration configuration;
		private readonly IErrorBackoffStrategy backoffStrategy;

		public MailService(IBus bus, IMessageProcessor processor, ILogger<MailService> logger, IErrorBackoffStrategy backoffStrategey) {
			this.bus = bus;
			this.processor = processor;
			this.logger = logger;
			this.backoffStrategy = backoffStrategey;
		}


		protected async override Task ExecuteAsync(CancellationToken cancellationToken) {
			setupRabbitBus();
			
			while (!cancellationToken.IsCancellationRequested) {
				await Task.Delay(1000);
				await backoffStrategy.HandleGlobalErrorState(
					() => subscriptionResult?.Dispose(),
					() => setupRabbitBus());
			}
			subscriptionResult?.Dispose();
			// also dispose the bus because it cannot be disposed by the DI framework
			bus?.Dispose();
		}

		private async Task processAsync(EmailMessage emailMessage) {
			try {
				await processor.ProcessAsync(emailMessage);
				logger.LogInformation($"Successfully processed email message {emailMessage.Id}");
			} catch(SingleDeliveryException e) {
				logger.LogError(e, "SimpleDeliveryError");
				throw e;
			} catch(GlobalDeliveryException e) {
				backoffStrategy.NotifyGlobalError();
				//await bus.PublishAsync(emailMessage);
				logger.LogError(e, "Global delivery problem.");
			}
		}

		private void setupRabbitBus() {
			subscriptionResult = bus.SubscribeAsync<EmailMessage>("mailqueue1", (emailMessage) => processAsync(emailMessage));
		}
	}
}
