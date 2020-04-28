using System;
using Microsoft.Extensions.Configuration;

namespace MiaPlaza.MailService {
public class SimpleErrorBackoffStrategyConfiguration {
		public int ErrorThreshold { get; set; }
		
		public int WaitTime { get; set; }

		public SimpleErrorBackoffStrategyConfiguration(IConfiguration configuration) {
			configuration.GetSection("SimpleBackoffStrategy").Bind(this);
			if(ErrorThreshold == 0) {
				throw new ArgumentException("SimpleErrorBackoffStrategyConfiguration:ErrorThreshold missing!");
			}
			if(WaitTime == 0) {
				throw new ArgumentException("SimpleErrorBackoffStrategyConfiguration:WaitTime missing!");
			}
		}
	}
}
