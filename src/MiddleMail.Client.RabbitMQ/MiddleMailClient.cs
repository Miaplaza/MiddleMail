using System;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
using MiaPlaza.MiddleMail.Model;

namespace MiaPlaza.MiddleMail.Client.RabbitMQ {

	/// <summary>
	/// A client for MiddleMail that sends emails via RabbitMQ. This needs a RabbitMQ Message Source on the service side.
	/// </summary>
	public class MiddleMailClient : IDisposable {

		private readonly IBus bus;

		public MiddleMailClient(IConfiguration configuration) {
			bus = EasyNetQ.RabbitHutch.CreateBus(configuration.GetValue<string>("RabbitMQConnectionString"));
		}

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
