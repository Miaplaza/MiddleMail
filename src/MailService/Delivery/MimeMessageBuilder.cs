using System;
using MimeKit;

namespace MiaPlaza.MailService.Delivery {
	
	public class MimeMessageBuilder : IMimeMessageBuilder {

		public MimeMessage Create(EmailMessage emailMessage) {
			if(emailMessage.PlainText == null) {
				throw new ArgumentException($"EmailMessage should always contains a Plaintext, but it is null.", nameof(emailMessage));
			}
			
			var mimeMessage = new MimeMessage();
			mimeMessage.From.Add(new MailboxAddress(emailMessage.FromName, emailMessage.FromEmail));
			mimeMessage.To.Add(new MailboxAddress(emailMessage.ToName, emailMessage.ToEmail));
			mimeMessage.Subject = emailMessage.Subject;

			if(emailMessage.HtmlText == null) {
				createBodyPlainText(emailMessage, mimeMessage);
			} else {
				createBodyMultipart(emailMessage, mimeMessage);
			}

			return mimeMessage;
		}

		private void createBodyPlainText(EmailMessage emailMessage, MimeMessage message) {
			message.Body = new TextPart("plain") {
				Text = emailMessage.PlainText
			};
		}

		private void createBodyMultipart(EmailMessage emailMessage, MimeMessage message) {
			var bodyBuilder = new BodyBuilder();
			bodyBuilder.HtmlBody = emailMessage.HtmlText;
			bodyBuilder.TextBody =  emailMessage.PlainText;

			message.Body = bodyBuilder.ToMessageBody();
		}
	}
}
