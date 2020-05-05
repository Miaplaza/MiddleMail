using System;
using EasyNetQ.Scheduling;
using MiaPlaza.MiddleMail.Delivery;
using MiaPlaza.MiddleMail.Delivery.Smtp;
using MiaPlaza.MiddleMail.MessageSource.RabbitMQ;
using MiaPlaza.MiddleMail.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MiaPlaza.MiddleMail {
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
				services.AddSingleton<MimeMessageConfiguration>();
				// prefetchcount = 10 so in case all messages throw an exception we can finish the prefetched messages on shutdown
				services.RegisterEasyNetQ("host=localhost;prefetchcount=10", x => x.Register<IScheduler, DelayedExchangeScheduler>());
				services.AddSingleton<IMailDeliverer, SmtpDeliverer>();
				services.AddSingleton<IMailStorage, MemoryStorage>();
				services.AddSingleton<IMimeMessageBuilder, MimeMessageBuilder>();
				services.AddSingleton<IMimeMessageSender, SmtpMimeMessageSender>();
				services.AddSingleton<IMessageProcessor, MessageProcessor>();
				services.AddSingleton<IRetryDelayStrategy, ExponentialRetryDelayStrategy>();
				services.AddSingleton<ExponentialRetryDelayConfiguration>();
				services.AddSingleton<IMessageSource, RabbitMQMessageSource>();
				services.AddStackExchangeRedisCache(options => {
					options.Configuration = "localhost";
					options.InstanceName = "SampleInstance";
				});
				services.AddHostedService<MailService>();
			});
	}
}
