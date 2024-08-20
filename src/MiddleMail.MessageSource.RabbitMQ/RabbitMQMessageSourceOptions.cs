using MiddleMail.Exceptions;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using MiddleMail.Model;

namespace MiddleMail.MessageSource.RabbitMQ {
	public class RabbitMQMessageSourceOptions {

		public const string SECTION = "MiddleMail:MessageSource:RabbitMQ";

		[Required]
		public string ConnectionString { get; set; }

		[Required]
		public string SubscriptionId { get; set; }

		[Required]
		public string Topic { get; set; }
	}
}
