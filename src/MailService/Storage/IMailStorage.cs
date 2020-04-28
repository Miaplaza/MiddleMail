using System.Threading.Tasks;

namespace MiaPlaza.MailService.Storage {
	public interface IMailStorage {
		Task SetProcessedAsync(EmailMessage emailMessage);
		
		Task SetSentAsync(EmailMessage emailMessage);

		Task SetErrorAsync(EmailMessage emailMessage, string errorMessage);

		Task<bool> TryGetProcessedCount(EmailMessage emailMessage, out int processedCount);
		Task<bool> TryGetSent(EmailMessage emailMessage, out bool sent);
		Task<bool> TryGetError(EmailMessage emailMessage, out string error);
	}
}
