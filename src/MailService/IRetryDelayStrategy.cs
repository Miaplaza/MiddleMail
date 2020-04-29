using System;
using System.Threading.Tasks;

namespace MiaPlaza.MailService {
	public interface IRetryDelayStrategy {

		int GetDelay(int retryCount);
	}
}
