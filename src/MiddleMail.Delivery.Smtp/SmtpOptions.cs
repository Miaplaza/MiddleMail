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

		// public SmtpConfiguration(IConfiguration configuration) {
		// 	configuration.GetSection("SMTP").Bind(this);
		// 	if(string.IsNullOrEmpty(Server)) {
		// 		throw new ConfigurationMissingException("SMTP__Server");
		// 	}
		// 	if(string.IsNullOrEmpty(Username)) {
		// 		throw new ConfigurationMissingException("SMTP__Username");
		// 	}
		// 	if(string.IsNullOrEmpty(Password)) {
		// 		throw new ConfigurationMissingException("SMTP__Password");
		// 	}
		// }
	}
}
