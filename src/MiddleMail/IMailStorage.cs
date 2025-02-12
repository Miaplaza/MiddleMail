using System.Threading.Tasks;
using MiddleMail.Model;

namespace MiddleMail {

	/// <summary>
	/// A storage to persist email activity. When calling any of the methods that set data, the order must be kept.
	/// Writing data does not need to be instantly and might not be reflected when reading it directly back.
	/// </summary>
	public interface IMailStorage {

		/// <summary>
		/// Store that an <see cref="EmailMessage" /> was processed.
		/// This can happen multiple times.
		/// </summary>
		Task SetProcessedAsync(EmailMessage emailMessage);

		/// <summary>
		/// Store that an <see cref="EmailMessage" /> was successfully sent.
		/// </summary>
		Task SetSentAsync(EmailMessage emailMessage);

		/// <summary>
		/// Store that an <see cref="EmailMessage" /> could not be sent because of an error.
		/// </summary>
		Task SetErrorAsync(EmailMessage emailMessage, string errorMessage);

		/// <summary>
		/// Get the stored value for sent for an <see cref="EmailMessage" />
		/// Return null if the <see cref="EmailMessage" /> could not be found in the storage.
		/// </summary>
		Task<bool?> GetSentAsync(EmailMessage emailMessage);

		/// <summary>
		/// Get the stored error for an <see cref="EmailMessage" />.
		/// Return null if the <see cref="EmailMessage" /> could not be found in the storage.
		/// </summary>
		Task<string> GetErrorAsync(EmailMessage emailMessag);
	}
}
