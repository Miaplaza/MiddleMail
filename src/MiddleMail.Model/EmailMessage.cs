using System;
using System.Collections.Generic;

namespace MiddleMail.Model {

	/// <summary>
	/// A data object describing an email to be sent.
	/// Clients will typically commit this type of object to MiddleMail that is then generating and sending out the corresponding email.
	/// </summary>
	public class EmailMessage {
		
		public Guid Id { get; set; }
		public (string name, string address) From { get; set; }
		public (string name, string address) To { get; set; }
		public List<(string name, string address)> Cc { get; set; }
		public (string name, string address)? ReplyTo { get; set; }
		public string Subject { get; set; }
		public string PlainText { get; set; }
		public string HtmlText { get; set; }

		/// <summary>
		/// A list of additional headers for the email
		/// </summary>
		public Dictionary<string, string> Headers { get; set; }

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

		public EmailMessage(Guid id, (string name, string address) from, (string name, string address) to,
			List<(string name, string address)> cc, (string name, string address)? replyTo, string subject,
			string plainText, string htmlText, List<string> tags, Dictionary<string, string> headers,
			int retryCount = 0, bool store = true) {

			if (String.IsNullOrEmpty(from.address)) {
				throw new ArgumentException("Must specify a from email address.", nameof(from));
			}
			if (String.IsNullOrEmpty(to.address)) {
				throw new ArgumentException("Must specify a to email address.", nameof(to));
			}
			if (String.IsNullOrEmpty(subject)) {
				throw new ArgumentException("Must specify a subject.", nameof(subject));
			}
			Id = id;
			From = from;
			To = to;
			Cc = cc;
			ReplyTo = replyTo;
			Subject = subject;
			PlainText = plainText;
			HtmlText = htmlText;
			Headers = headers ?? new Dictionary<string, string>();
			RetryCount = retryCount;
			Store = store;
			Tags = tags ?? new List<string>();
		}
	}
}
