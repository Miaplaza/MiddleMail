using MiaPlaza.MiddleMail.Exceptions;
using Microsoft.Extensions.Configuration;

namespace MiaPlaza.MiddleMail.Storage.ElasticSearch {
public class ElasticSearchStorageConfiguration {
		public string Uri { get; set; }
		public string Index { get; set; }

		public ElasticSearchStorageConfiguration(IConfiguration configuration) {
			configuration.GetSection("ElasticSearchStorage").Bind(this);
			if(string.IsNullOrEmpty(Uri)) {
				throw new ConfigurationMissingException("ElasticSearchStorage__Uri");
			}
			if(string.IsNullOrEmpty(Index)) {
				throw new ConfigurationMissingException("ElasticSearchStorage__Index");
			}
		}
	}
}
