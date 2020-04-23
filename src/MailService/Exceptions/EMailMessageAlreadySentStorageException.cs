using System;

namespace MiaPlaza.MailService.Exceptions {
	public class EMailMessageAlreadySentStorageException : Exception {

		public EMailMessageAlreadySentStorageException(EmailMessage emailMessage)
		 : base($"EmailMessage with Id {emailMessage.Id} has been already sent out.") { }
	}
}
