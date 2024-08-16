using System.ComponentModel.DataAnnotations;

namespace MiddleMail {
	public class MiddleMailOptions {
		public const string SECTION = "MiddleMail";

		[Required]
		public bool RateLimited { get; set; }

		public int LimitPerHour { get; set; }
	}
}
