﻿using System;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MiddleMail.Model;

namespace MiddleMail.Client.RabbitMQ {

	/// <summary>
	/// A client that connects to MiddleMail. It puts messages to be sent into a RabbitMQ queue that MiddleMail listens to.
	/// For this client to work, a RabbitMQ Message Source needs to be enabled in the MiddleMail server.
	/// This is class is thread safe and should be injected as a transient into the DI container.
	/// </summary>
	public class MiddleMailClient : IDisposable {

		private readonly IBus bus;
		private readonly ILogger<MiddleMailClient> logger;

		public MiddleMailClient(IOptions<RabbitMQOptions> options, ILogger<MiddleMailClient> logger) {
			bus = EasyNetQ.RabbitHutch.CreateBus(options.Value.ConnectionString);
			this.logger = logger;
		}

		/// <summary>
		/// Sends an email by publishing it to RabbitMQ. A published email will wait in a queue on RabbitMQ until 
		/// it is processed by a MiddleMail instance running a RabbitMQ Message Source.
		/// Returns true if the email is published to RabbitMQ successfully, false otherwise.
		/// </summary>
		public async Task<bool> SendEmailAsync(EmailMessage emailMessage) {
			try {
				await bus.PublishAsync(emailMessage);
				return true;
			} catch (Exception e) {
				logger.LogError("Failed to publish to rabbitmq queue.", e);
				return false;
			}
		}

		public void Dispose() {
			bus?.Dispose();
		}
	}
}
