using MiaPlaza.MiddleMail.Model;
using MimeKit;

namespace MiaPlaza.MiddleMail.Delivery {
	public interface IMimeMessageBuilder {
		MimeMessage Create(EmailMessage emailData);
	}
}
