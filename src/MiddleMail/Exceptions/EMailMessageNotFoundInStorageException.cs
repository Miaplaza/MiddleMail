using System;
using MiddleMail.Model;

namespace MiddleMail.Exceptions {
	public class EMailMessageNotFoundInStorageException : Exception {
		public EMailMessageNotFoundInStorageException(EmailMessage emailMessage)
		 : base($"Could not find EmailMessage with Id {emailMessage.Id}") { }
	}
}
