using System;
using MiaPlaza.MiddleMail.Model;

namespace MiaPlaza.MiddleMail.Exceptions {
	public class MimeMessageBuilderException : SingleProcessingException {

		public MimeMessageBuilderException(EmailMessage emailMessage, Exception innerException)
			: base($"Could not create MimeMessage for EmailMessage.", emailMessage, innerException) { }
	}
}
