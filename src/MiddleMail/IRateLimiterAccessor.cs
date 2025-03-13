using System.Threading.RateLimiting;

namespace MiddleMail {
	/// <summary>
	/// Exists so the RateLimiter can be dependency-injected for unit testing,
	/// without putting it on the IOptions object, which would create potential issues with configuration binding.
	/// </summary>
	public interface IRateLimiterAccessor {
		/// <summary>
		/// The rate limiter used to limit how many mails will be sent each minute.
		/// This is populated on startup if <see cref="RateLimiterOptions.RateLimited"/> is true,
		/// according to the <see cref="RateLimiterOptions.LimitPerMinute"/>
		/// </summary>
		public RateLimiter? RateLimiter { get; }
	}
}
