using System.Threading.Tasks;
using MiaPlaza.MiddleMail.Model;

namespace MiaPlaza.MiddleMail.Delivery {
	
	public interface IMailDeliverer {
		
		Task DeliverAsync(EmailMessage emailData);
	} 
}
