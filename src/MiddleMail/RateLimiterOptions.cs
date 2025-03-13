namespace MiddleMail {
	/// <summary>
	/// Options for the <see cref="RateLimiterAccessor"/> 
	/// </summary>
	public class RateLimiterOptions {
		public const string SECTION = "MiddleMail:RateLimiter";

		/// <summary>
		/// If non-null, rate limiting will be enabled such that
		/// no more than this many mails will be sent each minute.
		/// </summary>
		public int? LimitPerMinute { get; set; }
	}
}
