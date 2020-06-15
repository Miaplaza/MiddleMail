using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace MiddleMail {
	public class ExponentialBackoffOptions {
			
		public const string SECTION = "MiddleMail:ExponentialBackoff";

		[Required]
		public int Multiplicator { get; set; }
	}
}
