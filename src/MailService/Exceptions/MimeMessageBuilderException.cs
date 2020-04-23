using System;

namespace MiaPlaza.MailService.Exceptions {
	public class MimeMessageBuilderException : SingleDeliveryException {

		public MimeMessageBuilderException(EmailMessage emailMessage, Exception innerException)
			: base($"Could not create MimeMessage for EmailMessage.", emailMessage, innerException) { }
	}
}
