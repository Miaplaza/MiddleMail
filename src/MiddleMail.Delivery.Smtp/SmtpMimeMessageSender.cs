using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace MiaPlaza.MiddleMail.Delivery.Smtp {

	/// <summary>
	/// Sending of an <see cref="MimeKit.MimeMessage" /> via SMTP using one <see cref="MailKit.Net.Smtp.SmtpClient" />.async
	/// Since that is not thread safe, we synchronize access by using a semaphoreSlim.
	/// </summary>
	public class SmtpMimeMessageSender : IMimeMessageSender, IDisposable {
		private readonly SmtpConfiguration smtpConfig;
		private readonly SmtpClient smtpClient;
		private readonly SemaphoreSlim semaphoreSlim;

		public SmtpMimeMessageSender(SmtpConfiguration smtpConfig) {
			this.smtpConfig = smtpConfig;
			this.smtpClient = new SmtpClient();
			this.semaphoreSlim = new SemaphoreSlim(initialCount: 1, maxCount: 1);
		}

		private async Task connectAsync() {
			await this.smtpClient.ConnectAsync(smtpConfig.Server, smtpConfig.Port, SecureSocketOptions.None);

			// Note: since we don't have an OAuth2 token, disable
			// the XOAUTH2 authentication mechanism.
			this.smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");

			await this.smtpClient.AuthenticateAsync(smtpConfig.Username, smtpConfig.Password);
		}

		public async Task SendAsync(MimeMessage message) {
			await semaphoreSlim.WaitAsync();
			try {
				// initial connection
				if(!smtpClient.IsConnected) {
					await this.connectAsync();
				}
				
				// reconnect if we are not connected anymore
				try {
					await smtpClient.NoOpAsync();
				} catch (ProtocolException) {
					await this.connectAsync();
				}
				await smtpClient.SendAsync(message);
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
