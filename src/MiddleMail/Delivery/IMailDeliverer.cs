using System.Threading.Tasks;
using MiaPlaza.MiddleMail.Model;

namespace MiaPlaza.MiddleMail.Delivery {
	
	public interface IMailDeliverer {
		
		public Task DeliverAsync(EmailMessage emailData);
	} 
}
