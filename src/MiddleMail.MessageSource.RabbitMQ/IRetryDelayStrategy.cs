using System;
using System.Threading.Tasks;

namespace MiaPlaza.MiddleMail.MessageSource.RabbitMQ {
	public interface IRetryDelayStrategy {

		int GetDelay(int retryCount);
	}
}
