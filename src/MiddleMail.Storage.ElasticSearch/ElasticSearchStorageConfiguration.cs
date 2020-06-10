using MiddleMail.Exceptions;
using Microsoft.Extensions.Configuration;

namespace MiddleMail.Storage.ElasticSearch {

	/// <summary>
	/// Configuration for <see cref="ElasticSearchStorage" />
	/// </summary>
	public class ElasticSearchStorageConfiguration {

		/// <summary>
		/// The Uri to the ElasticSearch node
		/// </summary>
		public string Uri { get; set; }

		/// <summary>
		/// The index in which activity will be stored.
		/// </summary>
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
