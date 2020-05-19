using System;
using MiaPlaza.MiddleMail.Exceptions;
using MiaPlaza.MiddleMail.Model;

namespace MiaPlaza.MiddleMail.Delivery.Smtp.Exceptions {
	public class MimeMessageBuilderException : SingleProcessingException {

		public MimeMessageBuilderException(EmailMessage emailMessage, Exception innerException)
			: base($"Could not create MimeMessage for EmailMessage.", emailMessage, innerException) { }
	}
}
