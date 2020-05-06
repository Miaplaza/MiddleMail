using System;
using MiaPlaza.MiddleMail.Model;
using Nest;

namespace MiaPlaza.MiddleMail.Storage.ElasticSearch {

	[ElasticsearchType]
	public class EmailDocument : EmailMessage {
		
		public DateTime? Sent { get; set;  }
		public string Error { get; set; }
		public DateTime FirstProcessed { get; set; }
		public DateTime LastProcessed { get; set; }

		public EmailDocument(EmailMessage emailMessage, DateTime? sent = null, string error = null) 
			: base(emailMessage.Id, emailMessage.FromEmail, emailMessage.FromName, emailMessage.ToEmail, 
			emailMessage.ToName, emailMessage.Subject, emailMessage.PlainText, emailMessage.HtmlText, 
			emailMessage.RetryCount) {

			Sent = sent;
			Error = error;
			FirstProcessed = DateTime.UtcNow;
			LastProcessed = DateTime.UtcNow;
		}
	}
}
