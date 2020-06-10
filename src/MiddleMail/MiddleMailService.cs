using System;
using System.Threading;
using System.Threading.Tasks;
using MiddleMail.Exceptions;
using MiddleMail.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MiddleMail {

	/// <summary>
	/// A BackgroundService that consumes message from an <see cref="IMessageSource" /> and dispatches them to an
	/// <see cref="IMessageProcess" />.
	/// Implements a graceful shutdown by waiting for all processing tasks to finish work if cancellation is requested.
	/// </summary>
	public class MiddleMailService : BackgroundService {
		
		private IMessageProcessor processor;
		private readonly ILogger<MiddleMailService> logger;
		private readonly IMessageSource messageSource;
		private int consumerTasksPending;

		public MiddleMailService(IMessageProcessor processor, ILogger<MiddleMailService> logger, IMessageSource messageSource) {
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

				// wait for all consumer tasks to finish
				// this is important: a consumer task is only idempotent if does not get canceled
				while(consumerTasksPending != 0) {
					logger.LogInformation($"Waiting for {consumerTasksPending} Tasks to finish.");
					await Task.Delay(25);
				}
			}
		}

		private async Task processAsync(EmailMessage emailMessage ) {
			logger.LogDebug($"Start processing email message {emailMessage.Id}");
			Interlocked.Increment(ref consumerTasksPending);
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
				logger.LogError(e, $"Unexpected Exception while processing message {emailMessage.Id}");
				throw e;
			} finally {
				Interlocked.Decrement(ref consumerTasksPending);
			}
		}
	}
}
