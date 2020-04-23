using MimeKit;

namespace MiaPlaza.MailService.Delivery {
	public interface IMimeMessageBuilder {
		MimeMessage Create(EmailMessage emailData);
	}
}
