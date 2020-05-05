using System.Threading;
using System.Threading.Tasks;
using MimeKit;

namespace MiaPlaza.MiddleMail.Delivery {
	public interface IMimeMessageSender {

		Task SendAsync(MimeMessage message);
	}
}
