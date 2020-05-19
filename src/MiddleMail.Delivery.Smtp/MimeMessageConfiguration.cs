using MiaPlaza.MiddleMail.Exceptions;
using Microsoft.Extensions.Configuration;

namespace MiaPlaza.MiddleMail.Delivery.Smtp {

	/// <summary>
	/// Configuration for <see cref="MimeMessageBuilder" />
	/// </summary>
	public class MimeMessageConfiguration {

		/// <summary>
		/// The domain part of a Message-ID as in <foo@domain.part>
		/// </summary>
		public string MessageIdDomainPart { get; set; }

		public MimeMessageConfiguration(IConfiguration configuration) {
			configuration.GetSection("MimeMessage").Bind(this);
			if(string.IsNullOrEmpty(MessageIdDomainPart)) {
				throw new ConfigurationMissingException("MimeMessage__MessageIdDomainPart");
			}
		}
	}
}
