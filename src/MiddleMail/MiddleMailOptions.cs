using System.ComponentModel.DataAnnotations;

namespace MiddleMail {
	public class MiddleMailOptions {
		[Required]
		public bool RateLimited { get; set; }

		public int LimitPerHour { get; set; }
	}
}
