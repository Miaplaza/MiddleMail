using System;
using MimeKit;

namespace MiaPlaza.MailService.Exceptions {
	public class MimeMessageSenderException : SingleDeliveryException {

		public MimeMessageSenderException(EmailMessage emailMessage, MimeMessage mimeMessage, Exception innerException)
			: base($"Could not send MimeMessage {mimeMessage.Headers["Message-Id"]}.", emailMessage, innerException) { }
	}
}
