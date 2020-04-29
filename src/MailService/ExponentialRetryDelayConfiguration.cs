using System;
using Microsoft.Extensions.Configuration;

namespace MiaPlaza.MailService {
public class ExponentialRetryDelayConfiguration {
		public int Multiplicator { get; set; }
		
		public ExponentialRetryDelayConfiguration(IConfiguration configuration) {
			configuration.GetSection("ExponentialRetryDelay").Bind(this);
			if(Multiplicator == 0) {
				throw new ArgumentException("ExponentialRetryDelay:Multiplicator is not set.");
			}
		}
	}
}
