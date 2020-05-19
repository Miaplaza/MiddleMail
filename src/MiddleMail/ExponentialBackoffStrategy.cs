using System;
using Microsoft.Extensions.Logging;

namespace MiaPlaza.MiddleMail {

	/// <summary>
	/// An exponential backoff strategy.
	/// </summary>
	public class ExponentialBackoffStrategy : IBackoffStrategy {
		
		private readonly ExponentialBackoffConfiguration configuration;
		
		public ExponentialBackoffStrategy(ExponentialBackoffConfiguration configuration) {
			this.configuration = configuration;
		}
		
		public TimeSpan GetDelay(int retryCount) {
			return new TimeSpan(hours: 0, minutes: 0, seconds: (int)Math.Pow(2, retryCount) * configuration.Multiplicator);
		}
	}
}
