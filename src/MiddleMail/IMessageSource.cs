using System;
using System.Threading;
using System.Threading.Tasks;
using MiaPlaza.MiddleMail.Model;

namespace MiaPlaza.MiddleMail {

	/// <summary>
	/// A source that produces <see cref="EmailMessage" />
	/// </summary>
	public interface IMessageSource {

		/// <summary>
		/// Start listening to new messages and register the callback for processing.
		/// The MessageSource must expect the callback to throw an Exception indicating a problem with this message.
		/// While the error handling is up to the implementation, the <see cref="Emailmessage" /> should not be retried.
		/// </summary>
		void Start(Func<EmailMessage, Task> callback);

		/// <summary>
		/// Stop listing to new messages
		/// </summary>
		void Stop();

		/// <summary>
		/// Requeue a message for retry if a temporary processing error occurred
		/// </summary>
		Task RetryAsync(EmailMessage emailMessage);
	}
}
