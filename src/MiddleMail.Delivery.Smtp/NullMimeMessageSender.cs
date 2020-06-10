using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace MiddleMail.Delivery.Smtp {

	/// <summary>
	/// A stub that does not actually send anything but acts as an always successful <see cref="IMimeMessageSender" />
	/// </summary>
	public class NullMimeMessageSender : IMimeMessageSender {

		public Task SendAsync(MimeMessage message) {
			return Task.CompletedTask;
		}
	}
}
