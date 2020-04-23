using System.Threading.Tasks;

namespace MiaPlaza.MailService.Storage {
	public interface IMailStorage {
		Task SetHandledAsync(EmailMessage emailMessage);
		
		Task SetSentAsync(EmailMessage emailMessage);

		Task SetErrorAsync(EmailMessage emailMessage, string errorMessage);

		Task<bool> TryGetHandleCount(EmailMessage emailMessage, out int handleCount);
		Task<bool> TryGetSent(EmailMessage emailMessage, out bool sent);
		Task<bool> TryGetError(EmailMessage emailMessage, out string error);
	}
}
