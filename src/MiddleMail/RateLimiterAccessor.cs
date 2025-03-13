using System;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace MiddleMail {
	public class RateLimiterAccessor : IRateLimiterAccessor, IDisposable {
		private static readonly TimeSpan rateLimitWindow = TimeSpan.FromMinutes(1);

		public RateLimiter? RateLimiter { get; }

		public RateLimiterAccessor(IOptions<RateLimiterOptions> options) {
			RateLimiter = options.Value.LimitPerMinute is null
				? null
				: new FixedWindowRateLimiter(
					new FixedWindowRateLimiterOptions() {
						PermitLimit = options.Value.LimitPerMinute.Value,
						Window = rateLimitWindow,
					}
				);
		}

		public void Dispose() {
			RateLimiter?.Dispose();
		}
	}
}
