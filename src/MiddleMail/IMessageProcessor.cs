using System.Threading;
using System.Threading.Tasks;
using MiaPlaza.MiddleMail.Model;

namespace MiaPlaza.MiddleMail {
		
		/// <summary>
		/// Processes an <see cref="EmailMessage" />.
		///
		/// In contrast to an <see cref="IMailDeliverer" /> an <see cref="IMessageProcessor" /> must be idempotent.
		/// Additionally delivery should only include the actual delivery, all logging and other types of preprocessing
		/// unrelated to delivery should happen inside the <see cref="IMessageProcessor" />.
		/// </summary>
		public interface IMessageProcessor {

			Task ProcessAsync(EmailMessage emailMessage);
		}
}
