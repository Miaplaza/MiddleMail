using System;
using MiaPlaza.MiddleMail.Model;

namespace MiaPlaza.MiddleMail.Exceptions {

	/// <summary>
	/// The exception that is thrown when an <see cref="EmailMessage" /> could not be processed
	/// due to a general problem that is unrelated to the specific <see cref="EmailMessage" /> instance.
	/// The delivery will be retried.
	/// Throws a <see cref="SingleProcessingException" /> to indicate a problem with a specific message.
	/// </summary>
	public class GeneralProcessingException : Exception {

		public GeneralProcessingException() { }

		public GeneralProcessingException(EmailMessage emailMessage, Exception innerException)
			: base($"Exception when sending EmailMessage with Id {emailMessage.Id}", innerException) { }
	}
}
