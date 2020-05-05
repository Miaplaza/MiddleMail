using System;
using MiaPlaza.MiddleMail.Model;

namespace MiaPlaza.MiddleMail.Exceptions {

	/// <summary>
	/// The Exception that is thrown when an <see cref="EmailMessage" /> could not be processed
	/// due to a problem with that specific <see cref="EmailMessage" /> instance.
	/// The delivery will not be retried.static The Exception bubbles up to <see cref="IMessageSource" />.
	/// Throw a <see cref="GeneralDeliveryException" /> to indicate a general problem.
	/// </summary>
	public class SingleProcessingException : Exception {

		public SingleProcessingException() { }

		public SingleProcessingException(string message, EmailMessage emailMessage, Exception innerException)
			: base($"Exception when sending EmailMessage with Id {emailMessage.Id}: {message}", innerException) { }
	}
}
