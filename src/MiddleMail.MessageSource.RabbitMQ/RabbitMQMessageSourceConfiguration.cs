using MiddleMail.Exceptions;
using Microsoft.Extensions.Configuration;

namespace MiddleMail.MessageSource.RabbitMQ {
public class RabbitMQMessageSourceConfiguration {
		public string ConnectionString { get; set; }
		public string SubscriptionId { get; set; }

		public RabbitMQMessageSourceConfiguration(IConfiguration configuration) {
			configuration.GetSection("RabbitMQMessageSource").Bind(this);
			if(string.IsNullOrEmpty(ConnectionString)) {
				throw new ConfigurationMissingException("RabbitMQMessageSource__ConnectionString");
			}
			if(string.IsNullOrEmpty(SubscriptionId)) {
				throw new ConfigurationMissingException("RabbitMQMessageSource__SubscriptionId");
			}
			
		}
	}
}
