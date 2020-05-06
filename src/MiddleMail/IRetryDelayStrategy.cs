namespace MiaPlaza.MiddleMail {
	public interface IRetryDelayStrategy {

		int GetDelay(int retryCount);
	}
}
