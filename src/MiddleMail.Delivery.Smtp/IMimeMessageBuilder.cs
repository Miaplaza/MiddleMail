using MiaPlaza.MiddleMail.Model;
using MimeKit;

namespace MiaPlaza.MiddleMail.Delivery.Smtp {
	public interface IMimeMessageBuilder {
		MimeMessage Create(EmailMessage emailData);
	}
}
