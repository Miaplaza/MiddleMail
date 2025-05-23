using System;
using System.Threading.Tasks;
using MiddleMail.Exceptions;
using MiddleMail.Model;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace MiddleMail {

	/// <summary>
	/// A message processor that is idempotent by detecting duplicates using an <see cref="IDistributedCache" />.
	/// Activity is stored to an <see cref="IMailStorage" />
	/// </summary>
	/// <remarks>
	/// Many distributed system do not guarantee exactly-once delivery (such as RabbitMQ). We therefore only require 
	/// at-least-once delivery for an <see cref="IMessageSource" />. That means that a duplicate message could end up here.
	/// As it is rather critical that emails are delivered exactly-once we must detect duplicates. This is done by caching 
	/// successfully sent emails by their id and checking the cache before we even start processing. This requires that pro-
	/// cessing tasks always run until completion and are not interrupted.
	/// </remarks>
	public class MessageProcessor : IMessageProcessor {

		private readonly IMailDeliverer deliverer;
		private readonly IMailStorage storage;
		private readonly ILogger logger;
		private readonly IDistributedCache cache;

		public MessageProcessor(IMailDeliverer deliverer, IMailStorage storage, IDistributedCache cache, ILogger<MessageProcessor> logger) {
			this.deliverer = deliverer;
			this.storage = storage;
			this.logger = logger;
			this.cache = cache;
		}
		public async Task ProcessAsync(EmailMessage emailMessage) {
			string cached = null;
			try {
				cached = await cache.GetStringAsync(emailMessage.Id.ToString());
			} catch (Exception e) {
				throw new GeneralProcessingException(emailMessage, e);
			}

			if (cached != null) {
				logger.LogInformation($"Caught duplicate email {emailMessage.Id}");
				return;
			}

			if (emailMessage.Store) {
				await tryStoreOrLogAsync(() => storage.SetProcessedAsync(emailMessage));
			}
			try {
				await deliverer.DeliverAsync(emailMessage);
			} catch (Exception e) {
				if (emailMessage.Store) {
					await tryStoreOrLogAsync(() => storage.SetErrorAsync(emailMessage, e.ToString()));
				}
				throw;
			}

			// if the cache throws an exception we do not rethrow a GeneralProcessingException here because the message has already been delivered
			await cache.SetStringAsync(emailMessage.Id.ToString(), "t");
			if (emailMessage.Store) {
				await tryStoreOrLogAsync(() => storage.SetSentAsync(emailMessage));
			}
		}

		private async Task tryStoreOrLogAsync(Func<Task> storeFunc) {
			try {
				await storeFunc();
			} catch (Exception e) {
				logger.LogError(e, "Exception while storing EmailMessage.");
			}
		}
	}
}
