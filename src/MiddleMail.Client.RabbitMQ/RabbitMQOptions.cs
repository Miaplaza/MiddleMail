using System.ComponentModel.DataAnnotations;
using MiddleMail.Model;

namespace MiddleMail.Client.RabbitMQ {
	public class RabbitMQOptions {
		public const string SECTION = "RabbitMQ";

		[Required]
		public string ConnectionString { get; set; }

		/// <summary>
		/// The topic string used by RabbitMQ to route messages to different MiddleMail instances.
		/// For example, you might want one instance to handle "Bulk" and one to handle "Transactional" emails.
		/// This should match the topic you pass to the MiddleMail server in the RabbitMQMessageSourceOptions.
		/// </summary>
		[Required]
		public string Topic { get; set; }
	}
}
