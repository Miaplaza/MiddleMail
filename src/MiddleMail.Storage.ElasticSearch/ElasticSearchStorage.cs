using System.Linq;
using System;
using System.Threading.Tasks;
using MiddleMail.Model;
using Nest;
using MiddleMail.Exceptions;
using Microsoft.Extensions.Configuration;

namespace MiddleMail.Storage.ElasticSearch {

	/// <summary>
	/// A mail activity storage backed by ElasticSearch.
	/// Use <see cref="ElasticSearchStorageConfiguration" /> to configure the connection and index.
	/// </summary>
	public class ElasticSearchStorage : IMailStorage {
		
		private readonly ElasticClient client;
		private readonly ElasticSearchStorageConfiguration configuration;
		public ElasticSearchStorage(ElasticSearchStorageConfiguration configuration) {
			this.configuration = configuration;
			var node = new Uri(configuration.Uri);
			client = new ElasticClient(new ConnectionSettings(node));
			client.Indices.Create(index, index => index
				.Map<EmailDocument>(m => m
					.AutoMap()
				));
		}

		public async Task SetProcessedAsync(EmailMessage emailMessage) {
			await updateOrCreateAsync(emailMessage, 
				update: (EmailDocument emailDocument) => { },
				create: () => new EmailDocument(emailMessage));
		}

		public async Task SetSentAsync(EmailMessage emailMessage) {
			await updateOrCreateAsync(emailMessage, 
				update: (EmailDocument emailDocument) => {
					emailDocument.Sent = DateTime.UtcNow;
				},
				create: () => new EmailDocument(emailMessage, sent: DateTime.UtcNow));
		}

		public async Task SetErrorAsync(EmailMessage emailMessage, string errorMessage) {
			await updateOrCreateAsync(emailMessage, 
				update: (EmailDocument emailDocument) => {
					emailDocument.Error = errorMessage;
				},
				create: () => new EmailDocument(emailMessage, error: errorMessage));
		}

		private async Task updateOrCreateAsync(EmailMessage emailMessage, Action<EmailDocument> update, Func<EmailDocument> create) {
			var emailDocument = await searchDocument(emailMessage.Id);
			if(emailDocument != null) {
				if (emailDocument.Sent != null) {
					throw new EMailMessageAlreadySentStorageException(emailMessage);
				}
				update(emailDocument);
				emailDocument.RetryCount = emailMessage.RetryCount;
				emailDocument.LastProcessed = DateTime.UtcNow;
			} else {
				emailDocument = create();
			}
			var response = await this.client.IndexAsync(emailDocument, i => i.Index(index));
			if (!response.IsValid) {
				throw new Exception(response.ServerError.ToString());
			}
		}

		public async Task<bool?> GetSentAsync(EmailMessage emailMessage) {
			var existingDocument = await searchDocument(emailMessage.Id);
			return existingDocument?.Sent != null;
		}
		
		public async Task<string> GetErrorAsync(EmailMessage emailMessage) {
			var existingDocument = await searchDocument(emailMessage.Id);
			return existingDocument?.Error;		
		}

		private string index => configuration.Index;
		
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
