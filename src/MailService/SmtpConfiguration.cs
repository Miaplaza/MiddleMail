using Microsoft.Extensions.Configuration;

namespace MiaPlaza.MailService {
public class SmtpConfiguration {
		public string Server { get; set; }
		public int Port { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public bool Enabled { get; set; }

		public SmtpConfiguration(IConfiguration configuration) {
			configuration.GetSection("SMTP").Bind(this);
			// if(Enabled) {

			// }
		}
	}
}
