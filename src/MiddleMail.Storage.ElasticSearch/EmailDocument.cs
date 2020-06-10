using System;
using System.Collections.Generic;
using MiddleMail.Model;
using Nest;

namespace MiddleMail.Storage.ElasticSearch {

	[ElasticsearchType]
	/// <summary>
	/// Data for an ElasticSearch document to persist email data.
	/// </summary>
	public class EmailDocument {
		
		public DateTime? Sent { get; set;  }
		public string Error { get; set; }
		public DateTime FirstProcessed { get; set; }
		public DateTime LastProcessed { get; set; }
		public Guid Id { get; set; }
		public string FromName { get; set; }
		public string FromAddress { get; set; }
		public string ToName { get; set; }
		public string ToAddress { get; set; }
		public string ReplyToName { get; set; }
		public string ReplyToAddress { get; set; }
		public string Subject { get; set; }
		public string PlainText { get; set; }
		public string HtmlText { get; set; }

		public int RetryCount { get; set; }
		public List<string> Tags { get; set; }

		public EmailDocument(EmailMessage emailMessage, DateTime? sent = null, string error = null) {
			Id = emailMessage.Id;	

			FromName = emailMessage.From.name;
			FromAddress = emailMessage.From.address;
			ToName = emailMessage.To.name;
			ToAddress = emailMessage.To.address;
			ReplyToName = emailMessage.ReplyTo?.name;
			ReplyToAddress = emailMessage.ReplyTo?.address;

			Subject = emailMessage.Subject;
			PlainText = emailMessage.PlainText;
			HtmlText = emailMessage.HtmlText;
			Tags = emailMessage.Tags; 
			RetryCount = emailMessage.RetryCount; 

			Sent = sent;
			Error = error;
			FirstProcessed = DateTime.UtcNow;
			LastProcessed = DateTime.UtcNow;
		}
	}
}
