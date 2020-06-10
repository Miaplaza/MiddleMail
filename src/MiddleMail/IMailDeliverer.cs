using System.Threading.Tasks;
using MiddleMail.Model;

namespace MiddleMail {

	/// <summary>
	/// Delivers an email. This could happen through Smtp, Http or not at all. 
	/// Do not use this for duplicate detection or logging but treat this rather as the output of the processing pipeline.
	/// </summary>
	public interface IMailDeliverer {

		/// <summary>
		/// Delivers an <see cref="EmailMessage" />.
		/// 
		/// Throws a <see cref="GeneralProcessingException" /> if a general error occured and delivery should be retried later.
		/// 
		/// Throws a <see cref="SingleProcessingException" /> if an error related to that specific emailMessage occured and delivery should not be retried.
		/// </summary>
		Task DeliverAsync(EmailMessage emailMessage);
	} 
}
