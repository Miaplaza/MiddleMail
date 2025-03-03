using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;

namespace MiddleMail {
	/// <summary>
	/// Options for the <see cref="MiddleMailService"/> 
	/// </summary>
	public class MiddleMailOptions {
		public const string SECTION = "MiddleMail";

		/// <summary>
		/// Whether this instance of the MiddleMail service should be rate-limited.
		/// That is, whether no more than a specified number (<see cref="LimitPerMinute"/> )
		/// of mails should be sent each minute.
		/// </summary>
		[Required]
		public bool RateLimited { get; set; }

		/// <summary>
		/// If <see cref="RateLimited"/> behavior is enabled,
		/// no more than this many mails will be sent each minute.
		/// </summary>
		public int LimitPerMinute { get; set; }

		/// <summary>
		/// The rate limiter used to limit how many mails will be sent each minute.
		/// This is populated on startup if <see cref="RateLimited"/> is true,
		/// according to the <see cref="LimitPerMinute"/>
		/// </summary>
		public RateLimiter RateLimiter { get; set; }
	}
}
