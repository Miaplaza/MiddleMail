using System;
using MiaPlaza.MiddleMail.Model;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace MiaPlaza.MiddleMail.Delivery.Smtp {
	
	/// <summary>
	/// Builds MIME messages from an <see cref="EMailMessage" />.
	/// </summary>
	public class MimeMessageBuilder : IMimeMessageBuilder {

		private readonly MimeMessageConfiguration configuration;
		public MimeMessageBuilder(MimeMessageConfiguration configuration) {
			this.configuration = configuration;
		}

		/// <summary>
		/// Build a multipart or plaintext MIME message from <paramref name="emailMessage"/>.
		/// Note that <paramref name="emailMessage" /> must specify at least a plaintext version.
		/// </summary>
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

			mimeMessage.MessageId = $"{emailMessage.Id.ToString("N")}@{configuration.MessageIdDomainPart}>";
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
