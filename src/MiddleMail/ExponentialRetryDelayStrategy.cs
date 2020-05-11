using System;
using Microsoft.Extensions.Logging;

namespace MiaPlaza.MiddleMail {

	/// <summary>
	/// A delay strategy for exponential delay
	/// </summary>
	public class ExponentialRetryDelayStrategy : IRetryDelayStrategy {
		
		private readonly ExponentialRetryDelayConfiguration configuration;
		
		public ExponentialRetryDelayStrategy(ExponentialRetryDelayConfiguration configuration) {
			this.configuration = configuration;
		}
		
		public int GetDelay(int retryCount) {
			return 	(int)Math.Pow(2, retryCount) * configuration.Multiplicator;
		}
	}
}
