using System;
using MiaPlaza.MailService.Delivery;
using MiaPlaza.MailService.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MiaPlaza.MailService {
	class Program {
		public static void Main(string[] args) {
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
		 .ConfigureLogging(logging => {
				logging.ClearProviders();
				logging.AddConsole();
			})
			.ConfigureAppConfiguration((hostingContext, config) => {
				config.AddEnvironmentVariables(prefix: "SMTP__");
				config.AddEnvironmentVariables(prefix: "SimpleBackoffStrategy__");
			})
			.ConfigureServices((hostContext, services) => {
				services.AddSingleton<SmtpConfiguration>();
				services.AddSingleton<EasyNetQ.IBus>(EasyNetQ.RabbitHutch.CreateBus("host=localhost"));
				services.AddSingleton<IMailDeliverer, SmtpDeliverer>();
				services.AddSingleton<IMailStorage, MemoryStorage>();
				services.AddSingleton<IMimeMessageBuilder, MimeMessageBuilder>();
				services.AddSingleton<IMimeMessageSender, SmtpMimeMessageSender>();
				services.AddSingleton<IMessageProcessor, MessageProcessor>();
				services.AddSingleton<IErrorBackoffStrategy, SimpleErrorBackoffStrategy>();
				services.AddSingleton<SimpleErrorBackoffStrategyConfiguration>();
				services.AddHostedService<MailService>();
			});
	}
}
