using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MiddleMail {

	/// <summary>
	/// An exponential backoff strategy.
	/// </summary>
	public class ExponentialBackoffStrategy : IBackoffStrategy {
		
		private readonly ExponentialBackoffOptions options;
		
		public ExponentialBackoffStrategy(IOptions<ExponentialBackoffOptions> options) {
			this.options = options.Value;
		}
		
		public TimeSpan GetDelay(int retryCount) {
			return new TimeSpan(hours: 0, minutes: 0, seconds: (int)Math.Pow(2, retryCount) * options.Multiplicator);
		}
	}
}
