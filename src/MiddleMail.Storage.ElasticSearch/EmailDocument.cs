using System;
using MiaPlaza.MiddleMail.Model;
using Nest;

namespace MiaPlaza.MiddleMail.Storage.ElasticSearch {

	[ElasticsearchType]
	public class EmailDocument : EmailMessage {
		
		public bool Sent { get; set;  }
		public string Error { get; set; }

		public EmailDocument(EmailMessage emailMessage, bool sent, string error) 
			: base(emailMessage.Id, emailMessage.FromEmail, emailMessage.FromName, emailMessage.ToEmail, 
			emailMessage.ToName, emailMessage.Subject, emailMessage.PlainText, emailMessage.HtmlText, 
			emailMessage.RetryCount) {

			Sent = sent;
			Error = error;
		}
	}
}
