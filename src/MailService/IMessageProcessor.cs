using System.Threading.Tasks;

namespace MiaPlaza.MailService {

		public interface IMessageProcessor {
			Task HandleAsync(EmailMessage emailMessage);
		}
}
