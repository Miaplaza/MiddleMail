using System;

namespace MiddleMail {

	/// <summary>
	/// A strategy that calculates delay to retry a failed action solely based on the number of failed retries
	/// </summary>
	public interface IBackoffStrategy {

		/// <summary>
		/// Return the delay for the given retry in seconds.
		/// </summary>
		TimeSpan GetDelay(int retryCount);
	}
}
