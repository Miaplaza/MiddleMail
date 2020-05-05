using System.Threading;
using System.Threading.Tasks;
using MiaPlaza.MiddleMail.Model;

namespace MiaPlaza.MiddleMail {

		public interface IMessageProcessor {

			Task ProcessAsync(EmailMessage emailMessage);
		}
}
