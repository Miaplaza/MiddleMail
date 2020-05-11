namespace MiaPlaza.MiddleMail {

	/// <summary>
	/// A strategy that calculates delay to retry a failed action solely based on the number of failed retries
	/// </summary>
	public interface IRetryDelayStrategy {

		/// <summary>
		/// Returs the delay for the given retry in seconds
		/// </summary>
		int GetDelay(int retryCount);
	}
}
