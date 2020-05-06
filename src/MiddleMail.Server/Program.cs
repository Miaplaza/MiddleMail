using System;
using EasyNetQ.Scheduling;
using MiaPlaza.MiddleMail.Delivery;
using MiaPlaza.MiddleMail.Delivery.Smtp;
using MiaPlaza.MiddleMail.MessageSource.RabbitMQ;
using MiaPlaza.MiddleMail.Storage.ElasticSearch;
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
				.ConfigureServices((hostContext, services) => {
					services.AddSingleton<SmtpConfiguration>();
					services.AddSingleton<MimeMessageConfiguration>();
					services.AddSingleton<ElasticSearchStorageConfiguration>();
					services.AddSingleton<ExponentialRetryDelayConfiguration>();
					services.AddSingleton<RabbitMQMessageSourceConfiguration>();

					services.AddSingleton<IMailDeliverer, SmtpDeliverer>();
					services.AddSingleton<IMimeMessageBuilder, MimeMessageBuilder>();
					services.AddSingleton<IMimeMessageSender, SmtpMimeMessageSender>();

					services.AddSingleton<IMailStorage, ElasticSearchStorage>();
					services.AddSingleton<IMessageProcessor, MessageProcessor>();
					services.AddSingleton<IRetryDelayStrategy, ExponentialRetryDelayStrategy>();
					
					services.AddSingleton<IMessageSource, RabbitMQMessageSource>();
					services.AddStackExchangeRedisCache(options => {
						options.Configuration = "localhost";
						options.InstanceName = "SampleInstance";
					});
					services.AddHostedService<MailService>();
				});
	}
}
