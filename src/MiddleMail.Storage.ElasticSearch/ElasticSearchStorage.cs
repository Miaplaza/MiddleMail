using System.Linq;
using System;
using System.Threading.Tasks;
using MiaPlaza.MiddleMail.Model;
using Nest;
using MiaPlaza.MiddleMail.Exceptions;
using Microsoft.Extensions.Configuration;

namespace MiaPlaza.MiddleMail.Storage.ElasticSearch {
	public class ElasticSearchStorage : IMailStorage {
		
		private readonly ElasticClient client;
		private readonly IConfiguration configuration;
		public ElasticSearchStorage(IConfiguration configuration) {
			this.configuration = configuration;
			var node = new Uri(configuration.GetValue<string>("Uri"));
			client = new ElasticClient(new ConnectionSettings(node));
			client.Indices.Create(index, index => index.Map<EmailDocument>(m => m
				.AutoMap()));
		}

		public async Task SetProcessedAsync(EmailMessage emailMessage) {
			var existingDocument = await searchDocument(emailMessage.Id);

			string error = null;

			if(existingDocument != null) {
				if(existingDocument.Sent) {
					throw new EMailMessageAlreadySentStorageException(emailMessage);
				}
				error = existingDocument.Error;
			}

			var emailDocument = new EmailDocument(emailMessage, sent: false, error: error);

			await this.client.IndexAsync(emailDocument, i => i.Index(index));
		}

		public async Task SetSentAsync(EmailMessage emailMessage) {
			var emailDocument = await searchDocument(emailMessage.Id);

			if(emailDocument != null) {
				if (emailDocument.Sent) {
					throw new EMailMessageAlreadySentStorageException(emailMessage);
				}
				emailDocument.Sent = true;
			} else {
				emailDocument = new EmailDocument(emailMessage, sent: true, error: null);
			}
			
			await this.client.UpdateAsync<EmailDocument>(emailDocument.Id, u => u.Index(index).Doc(emailDocument));
		}

		public async Task SetErrorAsync(EmailMessage emailMessage, string errorMessage) {
			var emailDocument = await searchDocument(emailMessage.Id);

			if(emailDocument != null) {
				if (emailDocument.Sent) {
					throw new EMailMessageAlreadySentStorageException(emailMessage);
				}
				emailDocument.Error = errorMessage;
			} else {
				emailDocument = new EmailDocument(emailMessage, sent: false,  error: errorMessage);
			}

			await this.client.UpdateAsync<EmailDocument>(emailDocument.Id, u => u.Index(index).Doc(emailDocument));
		}

		public async Task<bool?> GetSentAsync(EmailMessage emailMessage) {
			var existingDocument = await searchDocument(emailMessage.Id);
			return existingDocument?.Sent;
		}
		
		public async Task<string> GetErrorAsync(EmailMessage emailMessage) {
			var existingDocument = await searchDocument(emailMessage.Id);
			return existingDocument?.Error;		
		}

		private string index => configuration.GetValue<string>("Index");
		
		private async Task<EmailDocument> searchDocument(Guid id) {
			var response = await client.SearchAsync<EmailDocument>(s => s
				.Index(index)
				.Query(q => q
				.Term(c => c
					.Field(p => p.Id)
					.Value(id))));
								
			return response.Documents.SingleOrDefault();
		}
	}
}
