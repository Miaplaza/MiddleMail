using System;
using Microsoft.Extensions.Logging;

namespace MiaPlaza.MiddleMail {

	public class ExponentialRetryDelayStrategy : IRetryDelayStrategy {
		
		private readonly ExponentialRetryDelayConfiguration configuration;
		private readonly ILogger<ExponentialRetryDelayStrategy> logger;
		
		public ExponentialRetryDelayStrategy(ExponentialRetryDelayConfiguration configuration, ILogger<ExponentialRetryDelayStrategy> logger) {
			this.configuration = configuration;
		}

		public int GetDelay(int retryCount) {
			return 	(int)Math.Pow(2, retryCount) * configuration.Multiplicator;
		}
	}
}
