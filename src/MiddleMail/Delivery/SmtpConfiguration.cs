using Microsoft.Extensions.Configuration;

namespace MiaPlaza.MiddleMail.Delivery {
public class SmtpConfiguration {
		public string Server { get; set; }
		public int Port { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }

		public SmtpConfiguration(IConfiguration configuration) {
			configuration.GetSection("SMTP").Bind(this);
			// if(Enabled) {

			// }
		}
	}
}
