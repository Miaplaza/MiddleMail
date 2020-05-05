using System.Threading.Tasks;
using MiaPlaza.MiddleMail.Model;

namespace MiaPlaza.MiddleMail.Storage {
	public interface IMailStorage {
		Task SetProcessedAsync(EmailMessage emailMessage);
		
		Task SetSentAsync(EmailMessage emailMessage);

		Task SetErrorAsync(EmailMessage emailMessage, string errorMessage);

		Task<bool> TryGetProcessedCount(EmailMessage emailMessage, out int processedCount);
		Task<bool> TryGetSent(EmailMessage emailMessage, out bool sent);
		Task<bool> TryGetError(EmailMessage emailMessage, out string error);
	}
}
