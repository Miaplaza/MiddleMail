#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.RateLimiting;
using MiddleMail.Exceptions;
using MiddleMail.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MiddleMail {

	/// <summary>
	/// A BackgroundService that consumes message from an <see cref="IMessageSource" /> and dispatches them to an
	/// <see cref="IMessageProcess" />.
	/// Implements a graceful shutdown by waiting for all processing tasks to finish work if cancellation is requested.
	/// </summary>
	public sealed class MiddleMailService : BackgroundService {
		public static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);
		public static readonly TimeSpan RateLimitDelay = TimeSpan.FromMinutes(1);
		
		private IMessageProcessor processor;
		private readonly ILogger<MiddleMailService> logger;
		private readonly IMessageSource messageSource;
		private int consumerTasksPending;
		private readonly MiddleMailOptions options;

		private readonly RateLimiter? rateLimiter;

		public MiddleMailService(IOptions<MiddleMailOptions> options, IMessageProcessor processor, ILogger<MiddleMailService> logger, IMessageSource messageSource) {
			this.processor = processor;
			this.logger = logger;
			this.consumerTasksPending = 0;
			this.messageSource = messageSource;
			this.options = options.Value;

			if (this.options.RateLimited) {
				rateLimiter = new FixedWindowRateLimiter(
					new FixedWindowRateLimiterOptions() {
						PermitLimit = this.options.LimitPerMinute,
						Window = RateLimitWindow,
					}
				);
			}
		}

		public override void Dispose() {
			base.Dispose();
			rateLimiter?.Dispose();
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

		/// <summary>
		/// If rate limiting is configured, acquires a lease,
		/// awaiting the RateLimitDelay at each failure to acquire
		/// until the lease is successfully acquired.
		/// This means that this function will not return
		/// until the number of emails to be sent is back below the rate limit.
		/// If rate limiting is not configured, returns null.
		/// (A using statement that uses null suceeds without any problem.)
		/// </summary>
		/// <returns></returns>
		private async Task<RateLimitLease?> acquireLease() {
			if (rateLimiter == default) { return null; }

			while (true) {
				RateLimitLease lease = await rateLimiter.AcquireAsync();
				if (lease.IsAcquired) { return lease; }

				await Task.Delay(RateLimitDelay);
			}
		}

		private async Task processAsync(EmailMessage emailMessage ) {
			using RateLimitLease? lease = await acquireLease();

			logger.LogDebug($"Start processing email message {emailMessage.Id}");
			Interlocked.Increment(ref consumerTasksPending);
			try {
				await processor.ProcessAsync(emailMessage);
				logger.LogDebug($"Successfully processed email message {emailMessage.Id}");
			} catch(SingleProcessingException e) {
				logger.LogError(e, $"Delivery error for message {emailMessage.Id}");
				throw;
			} catch(GeneralProcessingException e) {
				logger.LogError(e, $"General delivery problem for message {emailMessage.Id}");
				await messageSource.RetryAsync(emailMessage);
			} catch(Exception e) {
				logger.LogError(e, $"Unexpected Exception while processing message {emailMessage.Id}");
				throw;
			} finally {
				Interlocked.Decrement(ref consumerTasksPending);
			}
		}
	}
}
