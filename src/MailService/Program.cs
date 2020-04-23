using System;
using MiaPlaza.MailService.Delivery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MiaPlaza.MailService {
	class Program {
		public static void Main(string[] args) {
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
			.ConfigureAppConfiguration((hostingContext, config) => {
				config.AddEnvironmentVariables(prefix: "SMTP__");
			})
			.ConfigureServices((hostContext, services) => {
				services.AddSingleton<SmtpConfiguration>();
				services.AddSingleton<EasyNetQ.IBus>(EasyNetQ.RabbitHutch.CreateBus("host=localhost"));
				services.AddSingleton<IMailDeliverer, SmtpDeliverer>();
				services.AddSingleton<IMimeMessageBuilder, MimeMessageBuilder>();
				services.AddSingleton<IMimeMessageSender, SmtpMimeMessageSender>();
				services.AddHostedService<MailServiceWorker>();
			});
	}
}
