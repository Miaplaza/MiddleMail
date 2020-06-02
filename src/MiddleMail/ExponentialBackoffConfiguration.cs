using System;
using Microsoft.Extensions.Configuration;

namespace MiddleMail {
public class ExponentialBackoffConfiguration {
		public int Multiplicator { get; set; }
		
		public ExponentialBackoffConfiguration(IConfiguration configuration) {
			configuration.GetSection("ExponentialBackoff").Bind(this);
			if(Multiplicator == 0) {
				throw new ArgumentException("ExponentialBackoff:Multiplicator is not set.");
			}
		}
	}
}
