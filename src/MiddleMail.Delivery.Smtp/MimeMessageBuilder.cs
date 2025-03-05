using System;
using System.Linq;
using MiddleMail.Model;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Microsoft.Extensions.Options;

namespace MiddleMail.Delivery.Smtp {
	
	/// <summary>
	/// Builds MIME messages from an <see cref="EMailMessage" />.
	/// </summary>
	public class MimeMessageBuilder : IMimeMessageBuilder {

		private readonly MimeMessageOptions options;
		public MimeMessageBuilder(IOptions<MimeMessageOptions> options) {
			this.options = options.Value;
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
			mimeMessage.From.Add(new MailboxAddress(emailMessage.From.name, emailMessage.From.address));
			mimeMessage.To.Add(new MailboxAddress(emailMessage.To.name, emailMessage.To.address));

			emailMessage.Cc?.ForEach(cc => mimeMessage.Cc.Add(new MailboxAddress(cc.name, cc.address)));

			if(emailMessage.ReplyTo.HasValue) {
				mimeMessage.ReplyTo.Add(new MailboxAddress(emailMessage.ReplyTo.Value.name, emailMessage.ReplyTo.Value.address));
			}
			mimeMessage.Subject = emailMessage.Subject;

			if(emailMessage.HtmlText == null) {
				createBodyPlainText(emailMessage, mimeMessage);
			} else {
				createBodyMultipart(emailMessage, mimeMessage);
			}

			mimeMessage.MessageId = $"{emailMessage.Id:N}@{options.MessageIdDomainPart}>";

			setHeaders(emailMessage, mimeMessage);
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

		private void setHeaders(EmailMessage emailMessage, MimeMessage message) {
			foreach (var item in emailMessage.Headers) {
				// do not override any headers
				var header = message.Headers.FirstOrDefault(h => h.Field == item.Key);	
				if (header == null) {
					message.Headers.Add(item.Key, item.Value);
				}
			}
		}
	}
}
