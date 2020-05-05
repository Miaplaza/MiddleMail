using System;
using System.Threading;
using System.Threading.Tasks;
using MiaPlaza.MiddleMail.Exceptions;
using MiaPlaza.MiddleMail.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MiaPlaza.MiddleMail {
	public class MailService : BackgroundService {
		
		private IMessageProcessor processor;
		private readonly ILogger<MailService> logger;
		private readonly IMessageSource messageSource;
		private int consumerTasksPending;

		public MailService(IMessageProcessor processor, ILogger<MailService> logger, IMessageSource messageSource) {
			this.processor = processor;
			this.logger = logger;
			this.consumerTasksPending = 0;
			this.messageSource = messageSource;
		}

		protected async override Task ExecuteAsync(CancellationToken cancellationToken) {
			messageSource.Start(processAsync);
			try {
				await Task.Delay(Timeout.Infinite, cancellationToken);
			} catch (TaskCanceledException) {
				messageSource.Stop();

				// wait for all consumer task to finish
				// this is important: a consumer task is only idempotent if does not get canceled
				while(consumerTasksPending != 0) {
					logger.LogInformation($"Waiting for {consumerTasksPending} Tasks to finish.");
					await Task.Delay(25);
				}
			}
		}

		private async Task processAsync(EmailMessage emailMessage ) {
			Interlocked.Increment(ref consumerTasksPending);
			logger.LogDebug($"Start processing email message {emailMessage.Id}");
			try {
				await processor.ProcessAsync(emailMessage);
				logger.LogDebug($"Successfully processed email message {emailMessage.Id}");
			} catch(SingleProcessingException e) {
				logger.LogError(e, $"Delivery error for message {emailMessage.Id}");
				throw e;
			} catch(GeneralProcessingException e) {
				logger.LogError(e, $"General delivery problem for message {emailMessage.Id}");
				await messageSource.RetryAsync(emailMessage);
			} catch(Exception e) {
				logger.LogError(e, $"Exception while processing message {emailMessage.Id}");
				throw e;
			} finally {
				Interlocked.Decrement(ref consumerTasksPending);
			}
		}
	}
}
