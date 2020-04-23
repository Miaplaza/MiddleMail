using System.Threading.Tasks;

namespace MiaPlaza.MailService.Delivery {
	
	public interface IMailDeliverer {
		public Task DeliverAsync(EmailMessage emailData);
	} 
}
