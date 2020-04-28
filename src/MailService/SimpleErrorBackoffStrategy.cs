using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MiaPlaza.MailService {

	public class SimpleErrorBackoffStrategy : IErrorBackoffStrategy {

		private int globalErrorCount = 0;
		private readonly SimpleErrorBackoffStrategyConfiguration configuration;
		private readonly ILogger<SimpleErrorBackoffStrategy> logger;
		
		public SimpleErrorBackoffStrategy(SimpleErrorBackoffStrategyConfiguration configuration, ILogger<SimpleErrorBackoffStrategy> logger) {
			this.configuration = configuration;
			this.logger = logger;
		}

		public void NotifyGlobalError() {
			Interlocked.Increment(ref globalErrorCount);
		}
		public async Task HandleGlobalErrorState(Action shutdown, Action startup) {
			if(globalErrorCount > configuration.ErrorThreshold) {
				logger.LogWarning($"Suspending processing of messages for {configuration.WaitTime} milliseconds: Global error count exceeded threshold of {configuration.ErrorThreshold}.");
				shutdown();
				await Task.Delay(configuration.WaitTime);
				logger.LogWarning($"Restarting processing of messages.");
				System.Threading.Interlocked.Exchange(ref globalErrorCount, 0);
				startup();
			}
		}
	}
}
