using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MiaPlaza.MiddleMail.Client.RabbitMQ;

namespace MiaPlaza.EmailMessageGenerator {
	class Program {
		static void Main(int count = 10, bool invalid = false, string host = "localhost") {
			Console.WriteLine($"Generating {count} {(invalid ? "invalid " : string.Empty)}emails and sending the via rabbitmq");

			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string>{{"RabbitMQConnectionString", $"host={host}"}})
				.Build();

			using var client = new MiddleMailClient(configuration);
			for(int i = 0; i < count; i++) {
				var emailMessage = MiaPlaza.MiddleMail.Tests.FakerFactory.EmailMessageFaker.Generate();
				if(invalid) {
					emailMessage.Subject = null;
				}

				client.SendEmail(emailMessage);
			}
		}
	}
}
