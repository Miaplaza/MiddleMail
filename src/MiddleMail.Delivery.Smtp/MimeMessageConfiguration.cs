using System;
using MiaPlaza.MiddleMail.Exceptions;
using Microsoft.Extensions.Configuration;

namespace MiaPlaza.MiddleMail.Delivery.Smtp {
public class MimeMessageConfiguration {
		public string MessageIdDomainPart { get; set; }

		public MimeMessageConfiguration(IConfiguration configuration) {
			configuration.GetSection("MimeMessage").Bind(this);
			if(string.IsNullOrEmpty(MessageIdDomainPart)) {
				throw new ConfigurationMissingException("MimeMessage__MessageIdDomainPart");
			}
		}
	}
}
