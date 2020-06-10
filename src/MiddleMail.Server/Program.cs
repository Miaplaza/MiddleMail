using System;
using EasyNetQ.Scheduling;
using MiddleMail.Delivery;
using MiddleMail.Delivery.Smtp;
using MiddleMail.MessageSource.RabbitMQ;
using MiddleMail.Storage.ElasticSearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MiddleMail.Server {
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
				.ConfigureServices((hostContext, services) => {
					services.AddSingleton<SmtpConfiguration>();
					services.AddSingleton<MimeMessageConfiguration>();
					services.AddSingleton<ElasticSearchStorageConfiguration>();
					services.AddSingleton<ExponentialBackoffConfiguration>();
					services.AddSingleton<RabbitMQMessageSourceConfiguration>();

					services.AddSingleton<IMailDeliverer, SmtpDeliverer>();
					services.AddSingleton<IMimeMessageBuilder, MimeMessageBuilder>();
					if (Environment.GetEnvironmentVariable("DISABLE_SMTP") != null){
						services.AddSingleton<IMimeMessageSender, NullMimeMessageSender>();
					} else {
						services.AddSingleton<IMimeMessageSender, SmtpMimeMessageSender>();
					}
					services.AddSingleton<IMailStorage, ElasticSearchStorage>();
					services.AddSingleton<IMessageProcessor, MessageProcessor>();
					services.AddSingleton<IBackoffStrategy, ExponentialBackoffStrategy>();
					
					services.AddSingleton<IMessageSource, RabbitMQMessageSource>();
					services.AddStackExchangeRedisCache(options => {
						options.Configuration = Environment.GetEnvironmentVariable("REDIS_CONFIGURATION");
						options.InstanceName = Environment.GetEnvironmentVariable("REDIS_INSTANCE_NAME");

					});
					services.AddHostedService<MiddleMailService>();
				});
	}
}
