using System;
using System.Threading.Tasks;
using MiaPlaza.MailService.Delivery;
using MiaPlaza.MailService.Storage;
using Microsoft.Extensions.Logging;

namespace MiaPlaza.MailService {

	public class MessageProcessor : IMessageProcessor {

		private readonly IMailDeliverer deliverer;
		private readonly IMailStorage storage;
		private readonly ILogger logger;

		public MessageProcessor(IMailDeliverer deliverer, IMailStorage storage, ILogger<MessageProcessor> logger) {
			this.deliverer = deliverer;
			this.storage = storage;
			this.logger = logger;
		}

		public async Task ProcessAsync(EmailMessage emailMessage) {
			
			await tryStoreOrLogAsync(() => storage.SetProcessedAsync(emailMessage));
			try {
				await deliverer.DeliverAsync(emailMessage);
			} catch (Exception e){
				await tryStoreOrLogAsync(() => storage.SetErrorAsync(emailMessage, e.Message));
				throw e;
			}
			
			await tryStoreOrLogAsync(() => storage.SetSentAsync(emailMessage));
		}

		private async Task tryStoreOrLogAsync(Func<Task> storeFunc) {
			try {
				await storeFunc();
			} catch(Exception e) {
				logger.LogError(e, "Exception while storing EmailMessage.");
			}
		}
	}
}
