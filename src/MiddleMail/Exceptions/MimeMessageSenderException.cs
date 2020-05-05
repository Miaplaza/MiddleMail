using System;
using MiaPlaza.MiddleMail.Model;
using MimeKit;

namespace MiaPlaza.MiddleMail.Exceptions {
	public class MimeMessageSenderException : SingleProcessingException {

		public MimeMessageSenderException(EmailMessage emailMessage, MimeMessage mimeMessage, Exception innerException)
			: base($"Could not send MimeMessage {mimeMessage.Headers["Message-Id"]}.", emailMessage, innerException) { }
	}
}
