using System.Threading.Tasks;
using MiaPlaza.MiddleMail.Model;

namespace MiaPlaza.MiddleMail {
	public interface IMailStorage {
		Task SetProcessedAsync(EmailMessage emailMessage);
		
		Task SetSentAsync(EmailMessage emailMessage);

		Task SetErrorAsync(EmailMessage emailMessage, string errorMessage);

		Task<bool?> GetSentAsync(EmailMessage emailMessage);
		Task<string> GetErrorAsync(EmailMessage emailMessag);
	}
}
