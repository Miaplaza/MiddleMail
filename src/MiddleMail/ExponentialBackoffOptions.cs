using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace MiddleMail {
	public class ExponentialBackoffOptions {
			
		[Required]
		public const string SECTION = "MiddleMail:ExponentialBackoff";
		public int Multiplicator { get; set; }
	}
}
