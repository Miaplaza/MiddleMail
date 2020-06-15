using MiddleMail.Exceptions;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace MiddleMail.MessageSource.RabbitMQ {
	public class RabbitMQMessageSourceOptions {

		public const string SECTION = "MiddleMail:MessageSource:RabbitMQ";

		[Required]
		public string ConnectionString { get; set; }

		[Required]
		public string SubscriptionId { get; set; }
	}
}
