using System;
using System.Collections.Generic;

namespace MiaPlaza.MiddleMail.Model {

	/// <summary>
	/// A data object describing an email to be sent.
	/// Clients will typically commit this type of object to MiddleMail that is then generating and sending out the corresponding email.
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
		/// Whether this message should be stored by a MiddleMail.IMailStorage
		/// </summary>
		public bool Store { get; set; }

		/// <summary>
		/// Additional meta data tags. These tags are relevant to logging but should not be included into the email that is eventually sent out.
		/// </summary>
		public List<string> Tags { get; set; }

		public EmailMessage() {}

		public EmailMessage(Guid id, string fromEmail, string fromName, string toEmail, string toName, string subject, string plainText, string htmlText, List<string> tags, int retryCount = 0, bool store = true) {
			if (String.IsNullOrEmpty(fromEmail)) {
				throw new ArgumentException("Must specify a from email address.", nameof(fromEmail));
			}
			if (String.IsNullOrEmpty(toEmail)) {
				throw new ArgumentException("Must specify a to email address.", nameof(toEmail));
			}
			if (String.IsNullOrEmpty(subject)) {
				throw new ArgumentException("Must specify a subject.", nameof(subject));
			}
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
			Tags = tags ?? new List<string>();
		}
	}
}
