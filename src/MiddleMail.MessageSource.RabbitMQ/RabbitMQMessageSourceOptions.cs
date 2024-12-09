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

		/// <summary>
		/// The topic string used by RabbitMQ to route messages to different MiddleMail instances.
		/// For example, you might want one instance to handle "Bulk" and one to handle "Transactional" emails.
		/// This should match the topic you pass to the MiddleMailClient in the RabbitMQOptions.
		/// </summary>
		public string Topic { get; set; }
	}
}
