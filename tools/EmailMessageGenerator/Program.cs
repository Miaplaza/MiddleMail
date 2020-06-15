using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MiddleMail.Client.RabbitMQ;

namespace MiaPlaza.EmailMessageGenerator {
	class Program {
		static void Main(int count = 10, bool invalid = false, string host = "localhost") {
			Console.WriteLine($"Generating {count} {(invalid ? "invalid " : string.Empty)}emails and sending the via rabbitmq");

			var options = new RabbitMQOptions {
				ConnectionString = $"host={host}",
			};

			using var client = new MiddleMailClient(Options.Create(options));
			for(int i = 0; i < count; i++) {
				var emailMessage = MiddleMail.Tests.FakerFactory.EmailMessageFaker.Generate();
				if(invalid) {
					emailMessage.Subject = null;
				}

				client.SendEmail(emailMessage);
			}
		}
	}
}
