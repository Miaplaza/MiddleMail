using System.Threading;
using System.Threading.Tasks;
using MimeKit;

namespace MiaPlaza.MiddleMail.Delivery.Smtp {

	/// <summary>
	/// Send a <see cref="MimeKit.MimeMessage" />
	/// </summary>
	public interface IMimeMessageSender {
		Task SendAsync(MimeMessage message);
	}
}
