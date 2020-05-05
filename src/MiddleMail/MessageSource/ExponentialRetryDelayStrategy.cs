using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MiaPlaza.MiddleMail.MessageSource {

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
