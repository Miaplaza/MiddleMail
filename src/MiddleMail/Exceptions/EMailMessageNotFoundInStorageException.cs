using System;
using MiaPlaza.MiddleMail.Model;

namespace MiaPlaza.MiddleMail.Exceptions {
	public class EMailMessageNotFoundInStorageException : Exception {
		public EMailMessageNotFoundInStorageException(EmailMessage emailMessage)
		 : base($"Could not find EmailMessage with Id {emailMessage.Id}") { }
	}
}
