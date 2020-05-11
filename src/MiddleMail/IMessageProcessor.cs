using System.Threading;
using System.Threading.Tasks;
using MiaPlaza.MiddleMail.Model;

namespace MiaPlaza.MiddleMail {
		
		/// <summary>
		/// Processes an <see cref="EmailMessage" />.
		/// Processing could simply include delivery if idempotent.
		/// 
		/// Must be idempotent. 
		/// </summary>
		public interface IMessageProcessor {
			
			
			Task ProcessAsync(EmailMessage emailMessage);
		}
}
