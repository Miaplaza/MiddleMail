using System;
using MiddleMail.Exceptions;
using MiddleMail.Model;

namespace MiddleMail.Delivery.Smtp.Exceptions {
	public class MimeMessageBuilderException : SingleProcessingException {

		public MimeMessageBuilderException(EmailMessage emailMessage, Exception innerException)
			: base($"Could not create MimeMessage for EmailMessage.", emailMessage, innerException) { }
	}
}
