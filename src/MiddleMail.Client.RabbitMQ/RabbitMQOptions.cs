using System.ComponentModel.DataAnnotations;
using MiddleMail.Model;

namespace MiddleMail.Client.RabbitMQ {
	public class RabbitMQOptions {
		public const string SECTION = "RabbitMQ";

		[Required]
		public string ConnectionString { get; set; }

		[Required]
		public string Topic { get; set; }
	}
}
