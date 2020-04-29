using System.Threading;
using System.Threading.Tasks;
using MimeKit;

namespace MiaPlaza.MailService.Delivery {
	public interface IMimeMessageSender {

		Task SendAsync(MimeMessage message);
	}
}
