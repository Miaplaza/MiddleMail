using System.ComponentModel.DataAnnotations;

namespace MiddleMail.Client.RabbitMQ {
	public class RabbitMQOptions {
		public const string SECTION = "MiddleMail:Client:RabbitMQ";

		[Required]
		public string ConnectionString { get; set; }
	}
}
