using System;

namespace MiaPlaza.MailService {

	public class EmailMessage {

		public Guid Id { get; set; }
		public string FromEmail { get; set; }
		public string FromName { get; set; }
		public string ToEmail { get; set; }
		public string ToName { get; set; }
		public string Subject { get; set; }
		public string PlainText { get; set; }
		public string HtmlText { get; set; }

		public EmailMessage() {}

		public EmailMessage(Guid id, string fromEmail, string fromName, string toEmail, string toName, string subject, string plainText, string htmlText) {
			Id = id;
			FromEmail = fromEmail;
			FromName = fromName;
			ToEmail = toEmail;
			ToName = toName;
			Subject = subject;
			PlainText = plainText;
			HtmlText = htmlText;
		}
	}
}
