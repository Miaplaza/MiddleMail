using System;
using System.Threading.Tasks;
using MiaPlaza.MailService.Delivery;
using MiaPlaza.MailService.Storage;

namespace MiaPlaza.MailService {

	public class MessageProcessor : IMessageProcessor {

		private readonly IMailDeliverer deliverer;
		private readonly IMailStorage storage;

		public MessageProcessor(IMailDeliverer deliverer, IMailStorage storage) {
			this.deliverer = deliverer;
			this.storage = storage;
		}

		public async Task HandleAsync(EmailMessage emailMessage) {
			await storage.SetHandledAsync(emailMessage);
			try {
				await deliverer.DeliverAsync(emailMessage);
			} catch (Exception e){
				await storage.SetErrorAsync(emailMessage, e.Message);
				throw e;
			}
			
			await storage.SetSentAsync(emailMessage);
		}
	}
}
