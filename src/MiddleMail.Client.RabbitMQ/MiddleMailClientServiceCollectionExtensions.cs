using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MiddleMail.Client.RabbitMQ {
	public static class MiddleMailClientServiceCollectionExtensions {
		public static IServiceCollection AddMiddleMailClient(this IServiceCollection services, IConfigurationSection configurationSection) {
			if (services == null) {
				throw new ArgumentNullException(nameof(services));
			}

			services.AddOptions<RabbitMQOptions>()
					.Bind(configurationSection.GetSection(RabbitMQOptions.SECTION))
					.ValidateDataAnnotations();

			services.AddSingleton<MiddleMailClient>();

			return services;
		}

	}
}
