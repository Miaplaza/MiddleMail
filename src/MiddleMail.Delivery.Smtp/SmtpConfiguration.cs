using MiddleMail.Exceptions;
using Microsoft.Extensions.Configuration;

namespace MiddleMail.Delivery.Smtp {
	
	/// <summary>
	/// Configuration for <see cref="SmtpMimeMessageSender" />
	/// </summary>
	public class SmtpConfiguration {
		public string Server { get; set; }
		public int Port { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }

		public SmtpConfiguration(IConfiguration configuration) {
			configuration.GetSection("SMTP").Bind(this);
			if(string.IsNullOrEmpty(Server)) {
				throw new ConfigurationMissingException("SMTP__Server");
			}
			if(string.IsNullOrEmpty(Username)) {
				throw new ConfigurationMissingException("SMTP__Username");
			}
			if(string.IsNullOrEmpty(Password)) {
				throw new ConfigurationMissingException("SMTP__Password");
			}
		}
	}
}
