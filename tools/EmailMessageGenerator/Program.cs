using System;

namespace MiaPlaza.EmailMessageGenerator {
	class Program {
		static void Main(int count = 10, bool invalid = false) {
			Console.WriteLine($"Generating {count} {(invalid ? "invalid " : string.Empty)}emails and sending the via rabbitmq");

			var bus = EasyNetQ.RabbitHutch.CreateBus("host=localhost");
			for(int i = 0; i < count; i++) {
				var emailMessage = MiaPlaza.MailService.Tests.FakerFactory.EmailMessageFaker.Generate();
				if(invalid) {
					emailMessage.Subject = null;
				}
				bus.Publish(emailMessage);
			}
			bus.Dispose();
		}
	}
}
