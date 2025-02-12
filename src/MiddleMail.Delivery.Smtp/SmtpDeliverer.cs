using System;
using System.Threading;
using System.Threading.Tasks;
using MiddleMail.Delivery.Smtp.Exceptions;
using MiddleMail.Exceptions;
using MiddleMail.Model;
using MimeKit;

namespace MiddleMail.Delivery.Smtp {

	/// <summary>
	/// A delivery by SMTP
	/// </summary>
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
			} catch (Exception e) {
				throw new MimeMessageBuilderException(emailMessage, e);
			}
			try {
				await sender.SendAsync(mimeMessage);
			} catch (Exception e) {
				throw new GeneralProcessingException(emailMessage, e);
			}
		}
	}
}
