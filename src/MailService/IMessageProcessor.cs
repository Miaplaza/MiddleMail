using System.Threading;
using System.Threading.Tasks;

namespace MiaPlaza.MailService {

		public interface IMessageProcessor {

			Task ProcessAsync(EmailMessage emailMessage);
		}
}
