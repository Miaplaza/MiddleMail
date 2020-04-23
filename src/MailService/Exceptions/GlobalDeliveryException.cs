using System;
using MimeKit;

namespace MiaPlaza.MailService.Exceptions {
	public class GlobalDeliveryException : Exception {

		public GlobalDeliveryException(EmailMessage emailMessage, MimeMessage mimeMessage, Exception innerException)
			: base($"Exception when sending EmailMessage with Id {emailMessage.Id}", innerException) { }
	}
}
