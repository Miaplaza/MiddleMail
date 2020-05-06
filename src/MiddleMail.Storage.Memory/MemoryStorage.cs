using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System;
using MiaPlaza.MiddleMail.Exceptions;
using MiaPlaza.MiddleMail.Model;

namespace MiaPlaza.MiddleMail.Storage.Memory {

	/// <summary>
	/// A reference implementation for 
	/// </summary>
	public class MemoryStorage : IMailStorage {
			
		private readonly ConcurrentDictionary<EmailMessage, (bool sent, string error)> storage;

		public MemoryStorage() {
			this.storage = new ConcurrentDictionary<EmailMessage, (bool sent, string error)>();
		}

		public Task SetProcessedAsync(EmailMessage emailMessage) {
			storage.AddOrUpdate(emailMessage, 
				addValue: (false, null), 
				updateValueFactory: (EmailMessage key, (bool sent, string error) value) => {
					if (value.sent) {
						throw new EMailMessageAlreadySentStorageException(emailMessage);
					}
					return (sent: false, error: value.error);
				}
			);
			return Task.CompletedTask;
		}

		public Task SetSentAsync(EmailMessage emailMessage) {
			storage.AddOrUpdate(emailMessage, 
				addValue: (true, null), 
				updateValueFactory: (EmailMessage key, (bool sent, string error) value) => {
					if (value.sent) {
						throw new EMailMessageAlreadySentStorageException(emailMessage);
					}
					return (sent: true, error: value.error);
				}
			);
			return Task.CompletedTask;
		}

		public Task SetErrorAsync(EmailMessage emailMessage, string errorMessage) {
			storage.AddOrUpdate(emailMessage, 
				addValue: (false, errorMessage),
				updateValueFactory: (EmailMessage key, (bool sent, string error) value) => {
					if (value.sent) {
						throw new EMailMessageAlreadySentStorageException(emailMessage);
					}
					return (sent: false, error: errorMessage);
				}
			);
			return Task.CompletedTask;
		}
		
		public Task<bool?> GetSentAsync(EmailMessage emailMessage) {
			var found = storage.TryGetValue(emailMessage, out var value);
			if (found) {
				return Task.FromResult((bool?)value.sent);
			}
			return Task.FromResult((bool?)null);
		}
		public Task<string> GetErrorAsync(EmailMessage emailMessage) {
			var found = storage.TryGetValue(emailMessage, out var value);
			if (found) {
				return Task.FromResult(value.error);
			}
			return Task.FromResult((string)null);
		}
	}
}
