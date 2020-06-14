using System;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Options;
using MiddleMail.Model;

namespace MiddleMail.Client.RabbitMQ {

	/// <summary>
	/// A client that connects to MiddleMail. It puts messages to be sent into a RabbitMQ queue that MiddleMail listens to.
	/// For this client to work, a RabbitMQ Message Source needs to be enabled in the MiddleMail server.
	/// This is class is thread safe and should be injected as a singleton into the DI container.
	/// </summary>
	public class MiddleMailClient : IDisposable {

		private readonly IBus bus;

		public MiddleMailClient(IOptions<MiddleMailClientRabbitMQOptions> options) {
			bus = EasyNetQ.RabbitHutch.CreateBus(options.Value.ConnectionString);
		}

		/// <summary>
		/// Sends an email by publishing it to RabbitMQ. A published email will wait in a queue on RabbitMQ until 
		/// it is processed by a MiddleMail instance running a RabbitMQ Message Source.
		/// </summary>
		public async Task SendEmailAsync(EmailMessage emailMessage) {
			await bus.PublishAsync(emailMessage).ConfigureAwait(false);
		}

		public void SendEmail(EmailMessage emailMessage) {
			bus.Publish(emailMessage);
		}
		
		public void Dispose() {
			bus?.Dispose();
		}
	}
}
