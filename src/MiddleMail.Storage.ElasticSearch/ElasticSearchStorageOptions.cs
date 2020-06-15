using MiddleMail.Exceptions;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace MiddleMail.Storage.ElasticSearch {

	/// <summary>
	/// Configuration for <see cref="ElasticSearchStorage" />
	/// </summary>
	public class ElasticSearchStorageOptions {

		public const string SECTION = "MiddleMail:Storage:ElasticSearch";

		/// <summary>
		/// The Uri to the ElasticSearch node
		/// </summary>
		[Required]
		public string Uri { get; set; }

		/// <summary>
		/// The index in which activity will be stored.
		/// </summary>
		[Required]
		public string Index { get; set; }
	}
}
