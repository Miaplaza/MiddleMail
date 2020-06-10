using System;
using MiddleMail.Exceptions;
using MiddleMail.Model;
using MimeKit;

namespace MiddleMail.Delivery.Smtp.Exceptions {
	public class MimeMessageSenderException : SingleProcessingException {

		public MimeMessageSenderException(EmailMessage emailMessage, MimeMessage mimeMessage, Exception innerException)
			: base($"Could not send MimeMessage {mimeMessage.Headers["Message-Id"]}.", emailMessage, innerException) { }
	}
}
