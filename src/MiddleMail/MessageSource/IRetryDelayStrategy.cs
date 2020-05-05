using System;
using System.Threading.Tasks;

namespace MiaPlaza.MiddleMail.MessageSource {
	public interface IRetryDelayStrategy {

		int GetDelay(int retryCount);
	}
}
