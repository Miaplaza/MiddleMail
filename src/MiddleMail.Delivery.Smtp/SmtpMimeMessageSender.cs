using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MiddleMail.Delivery.Smtp {

	/// <summary>
	/// Sends <see cref="MimeKit.MimeMessage" /> via SMTP.
	/// This class is thread safe. 
	/// </summary>
	public class SmtpMimeMessageSender : IMimeMessageSender, IDisposable {
		private readonly SmtpOptions options;
		private readonly SmtpClient smtpClient;

		/// Since <see cref="MailKit.Net.Smtp.SmtpClient" /> is not thread safe, we synchronize access to it by a semaphoreSlim.
		private readonly SemaphoreSlim semaphoreSlim;

		private readonly ILogger<SmtpMimeMessageSender> logger;

		public SmtpMimeMessageSender(IOptions<SmtpOptions> smtpConfig, ILogger<SmtpMimeMessageSender> logger) {
			this.options = smtpConfig.Value;
			this.smtpClient = new SmtpClient();
			this.semaphoreSlim = new SemaphoreSlim(initialCount: 1, maxCount: 1);
			this.logger = logger;
		}

		private async Task connectAsync() {
			await this.smtpClient.ConnectAsync(options.Server, options.Port, SecureSocketOptions.None);

			// Note: since we don't have an OAuth2 token, disable
			// the XOAUTH2 authentication mechanism.
			this.smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
			try {
				await this.smtpClient.AuthenticateAsync(options.Username, options.Password);
			} catch(AuthenticationException e) {
				// if authentication fails we close the connection and try again next time
				await this.smtpClient.DisconnectAsync(quit: true);
				throw e;
			}
		}

		public async Task SendAsync(MimeMessage message) {
			await semaphoreSlim.WaitAsync();
			try {				
				try {
					await smtpClient.SendAsync(message);
				} catch(Exception e) when (e is SmtpProtocolException | e is IOException | e is ServiceNotConnectedException |  e is SmtpCommandException) {
					// reconnect and try again
					// there are a multitude of reasons why this can fail even if the smtp connection was fine before:
					// e.g. limits exim4 the number of messages per connection smtp_accept_max_per_connection
					await smtpClient.DisconnectAsync(quit: true);
					await this.connectAsync();
					await smtpClient.SendAsync(message);
				}
			} finally {
				semaphoreSlim.Release();
			}
		}
	
		public void Dispose(){
			smtpClient.Disconnect(quit: true);
			semaphoreSlim.Dispose();
			smtpClient.Dispose();
		}
	}
}
