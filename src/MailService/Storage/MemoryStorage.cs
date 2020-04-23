using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System;
using MiaPlaza.MailService.Exceptions;

namespace MiaPlaza.MailService.Storage {
	public class MemoryStorage : IMailStorage {
			
		private readonly ConcurrentDictionary<EmailMessage, (bool sent, int handleCount, string error)> storage;

		public MemoryStorage() {
			this.storage = new ConcurrentDictionary<EmailMessage, (bool sent, int handleCount, string error)>();
		}

		public Task SetHandledAsync(EmailMessage emailMessage) {
			storage.AddOrUpdate(emailMessage, 
				addValue: (false, 1, null), 
				updateValueFactory: (EmailMessage key, (bool sent, int handleCount, string error) value) => {
					if (value.sent) {
						throw new EMailMessageAlreadySentStorageException(emailMessage);
					}
					return (sent: false, handleCount: value.handleCount+1, error: value.error);
				}
			);
			return Task.CompletedTask;
		}

		public Task SetSentAsync(EmailMessage emailMessage) {
			if(!storage.ContainsKey(emailMessage)) {
				throw new EMailMessageNotFoundInStorageException(emailMessage);
			}
			storage.AddOrUpdate(emailMessage, 
				// add value is ignored becaus we never remove from storage and we know that the key exists
				addValue: (true, 1, null), 
				updateValueFactory: (EmailMessage key, (bool sent, int handleCount, string error) value) => {
					if (value.sent) {
						throw new EMailMessageAlreadySentStorageException(emailMessage);
					}
					return (sent: true, handleCount: value.handleCount, error: value.error);
				}
			);
			return Task.CompletedTask;
		}

		public Task SetErrorAsync(EmailMessage emailMessage, string errorMessage) {
			if(!storage.ContainsKey(emailMessage)) {
				throw new EMailMessageNotFoundInStorageException(emailMessage);
			}
			storage.AddOrUpdate(emailMessage, 
				// add value is ignored becaus we never remove from storage and we know that the key exists
				addValue: (false, 1, "ERROR"),
				updateValueFactory: (EmailMessage key, (bool sent, int handleCount, string error) value) => {
					if (value.sent) {
						throw new EMailMessageAlreadySentStorageException(emailMessage);
					}
					return (sent: false, handleCount: value.handleCount, error: errorMessage);
				}
			);
			return Task.CompletedTask;
		}
		
		public Task<bool> TryGetHandleCount(EmailMessage emailMessage, out int handleCount) {
			var found = storage.TryGetValue(emailMessage, out var value);
			handleCount = value.handleCount;
			return Task.FromResult(found);
		}
		public Task<bool> TryGetSent(EmailMessage emailMessage, out bool sent) {
			var found = storage.TryGetValue(emailMessage, out var value);
			sent = value.sent;
			return Task.FromResult(found);
		}
		public Task<bool> TryGetError(EmailMessage emailMessage, out string error) {
			var found = storage.TryGetValue(emailMessage, out var value);
			error = value.error;
			return Task.FromResult(found);
		}
	}
}
