using System.ComponentModel.DataAnnotations;

namespace MiddleMail.Delivery.Smtp {
	
	/// <summary>
	/// Configuration for <see cref="SmtpMimeMessageSender" />
	/// </summary>
	public class SmtpOptions {

		public const string SECTION = "MiddleMail:Delivery:Smtp";

		[Required]
		public string Server { get; set; }

		[Required]
		public int Port { get; set; }

		[Required]
		public string Username { get; set; }

		[Required]
		public string Password { get; set; }
	}
}
