using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace MiaPlaza.MiddleMail.Delivery.Smtp {
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
					if(!smtpClient.IsConnected) {
						await this.connectAsync();
					}
					
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
