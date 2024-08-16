using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using MiddleMail.Client.RabbitMQ;
using MiddleMail.Model;

namespace MiaPlaza.EmailMessageGenerator {
	class Program {
		static void Main(int count = 10, bool invalid = false, string host = "localhost", MailUrgency urgency = MailUrgency.Bulk) {
			Console.WriteLine($"Generating {count} {urgency}={urgency.Topic()} {(invalid ? "invalid " : string.Empty)}emails and sending the via rabbitmq");

			var options = new RabbitMQOptions {
				ConnectionString = $"host={host}",
				Urgency = urgency,
			};

			using var client = new MiddleMailClient(Options.Create(options), new NullLogger<MiddleMailClient>());
			for(int i = 0; i < count; i++) {
				var emailMessage = MiddleMail.Tests.FakerFactory.EmailMessageFaker.Generate();
				if(invalid) {
					emailMessage.Subject = null;
				}

				client.SendEmailAsync(emailMessage).GetAwaiter().GetResult();
			}
		}
	}
}
