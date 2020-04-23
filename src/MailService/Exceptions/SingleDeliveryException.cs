using System;
using MimeKit;

namespace MiaPlaza.MailService.Exceptions {
	public class SingleDeliveryException : Exception {

		public SingleDeliveryException(string message, EmailMessage emailMessage, Exception innerException)
			: base($"Exception when sending EmailMessage with Id {emailMessage.Id}: {message}", innerException) { }
	}
}
