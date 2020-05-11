using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace MiaPlaza.MiddleMail.Delivery.Smtp {

	/// <summary>
	/// A stub that does not do anything but acts as an always successfull <see cref="IMimeMessageSender" />
	/// </summary>
	public class NullMimeMessageSender : IMimeMessageSender {

		public Task SendAsync(MimeMessage message) {
			return Task.CompletedTask;
		}
	}
}
