using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiaPlaza.MailService {

	public interface IMessageSource {

		/// <summary>
		/// Start listening to new messages and register the callback tfor processing
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
