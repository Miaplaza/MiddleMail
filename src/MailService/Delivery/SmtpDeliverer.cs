using System;
using System.Threading;
using System.Threading.Tasks;
using MiaPlaza.MailService.Exceptions;
using MimeKit;

namespace MiaPlaza.MailService.Delivery {
	
	public class SmtpDeliverer : IMailDeliverer {
		private readonly IMimeMessageBuilder builder;
		private readonly IMimeMessageSender sender;
		public SmtpDeliverer(IMimeMessageBuilder builder, IMimeMessageSender sender) {
			this.builder = builder;
			this.sender = sender;
		}

		public async Task DeliverAsync(EmailMessage emailMessage) {
			MimeMessage mimeMessage;
			try {
				mimeMessage = builder.Create(emailMessage);
			} catch(Exception e) {
				throw new MimeMessageBuilderException(emailMessage, e);
			}
			try {
				await sender.SendAsync(mimeMessage);
			} catch (InvalidOperationException e) {
				throw new MimeMessageSenderException(emailMessage, mimeMessage, e);
			} catch (Exception e) {
				throw new GeneralProcessingException(emailMessage, mimeMessage, e);
			}
		}
	}
}
