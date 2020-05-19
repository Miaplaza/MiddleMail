using MiaPlaza.MiddleMail.Model;
using MimeKit;

namespace MiaPlaza.MiddleMail.Delivery.Smtp {

	/// <summary>
	/// Creates a <see cref="MimeKit.MimeMessage" /> from an <see cref="EmailMessage" />
	/// </summary>
	public interface IMimeMessageBuilder {
		MimeMessage Create(EmailMessage emailData);
	}
}
