using System;
using System.Threading.Tasks;

namespace MiaPlaza.MailService {
	public interface IErrorBackoffStrategy {

		void NotifyGlobalError();

		Task HandleGlobalErrorState(Action shutdown, Action startup);
	}
}
