using System;
using MiddleMail.Model;

namespace MiddleMail.Exceptions {
	public class EMailMessageAlreadySentStorageException : Exception {

		public EMailMessageAlreadySentStorageException(EmailMessage emailMessage)
		 : base($"EmailMessage with Id {emailMessage.Id} has been already sent out.") { }
	}
}
