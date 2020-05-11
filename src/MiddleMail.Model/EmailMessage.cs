using System;
using System.Collections.Generic;

namespace MiaPlaza.MiddleMail.Model {

	/// <summary>
	/// A data object for email data.
	/// </summary>
	public class EmailMessage {
		
		public Guid Id { get; set; }
		public string FromEmail { get; set; }
		public string FromName { get; set; }
		public string ToEmail { get; set; }
		public string ToName { get; set; }
		public string Subject { get; set; }
		public string PlainText { get; set; }
		public string HtmlText { get; set; }

		public int RetryCount { get; set; }

		/// <summary>
		/// If this message should be stored.
		/// </summary>
		public bool Store { get; set; }

		/// <summary>
		/// Additional meta data tags. Should ne be included when sending this email.
		/// </summary>
		public List<string> Tags { get; set; }

		public EmailMessage() {}

		public EmailMessage(Guid id, string fromEmail, string fromName, string toEmail, string toName, string subject, string plainText, string htmlText, List<string> tags, int retryCount = 0, bool store = true) {
			Id = id;
			FromEmail = fromEmail;
			FromName = fromName;
			ToEmail = toEmail;
			ToName = toName;
			Subject = subject;
			PlainText = plainText;
			HtmlText = htmlText;
			RetryCount = retryCount;
			Store = store;
			Tags = tags;
		}
	}
}
