using System.Threading.Tasks;
using MiaPlaza.MiddleMail.Model;

namespace MiaPlaza.MiddleMail.Delivery {

	/// <summary>
	/// Delivers an email. This could happen through Smtp, Http or not at all. 
	/// Do not use this for duplicate detection or logging but treat this rather as the output of the processing pipeline.
	/// </summary>
	public interface IMailDeliverer {

		/// <summary>
		/// Delivers an <see cref="EmailMessage" />.
		/// 
		/// Throws a <see cref="GeneralProcessingException" /> if a general error occured and delivery should be retried later.async
		/// 
		/// Throws a <see cref="SingleProcessingException" /> if an error ony related to that specific emailMessage occured and delivery should not be retried.
		/// </summary>
		Task DeliverAsync(EmailMessage emailMessage);
	} 
}
