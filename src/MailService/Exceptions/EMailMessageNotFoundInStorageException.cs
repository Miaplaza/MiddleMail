using System;

namespace MiaPlaza.MailService.Exceptions {
	public class EMailMessageNotFoundInStorageException : Exception {
		public EMailMessageNotFoundInStorageException(EmailMessage emailMessage)
		 : base($"Could not find EmailMessage with Id {emailMessage.Id}") { }
	}
}
