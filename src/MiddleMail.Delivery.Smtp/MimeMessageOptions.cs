using System.ComponentModel.DataAnnotations;

namespace MiddleMail.Delivery.Smtp {

	/// <summary>
	/// Configuration for <see cref="MimeMessageBuilder" />
	/// </summary>
	public class MimeMessageOptions {

		public const string SECTION = "MiddleMail:Delivery:MimeMessage";

		/// <summary>
		/// The domain part of a Message-ID as in <foo@domain.part>
		/// </summary>
		[Required]
		public string MessageIdDomainPart { get; set; }
	}
}
