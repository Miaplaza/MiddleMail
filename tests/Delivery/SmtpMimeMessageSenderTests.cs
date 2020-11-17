using System.Threading;
using System;
using System.Net;
using System.Collections.Generic;
using MiddleMail.Delivery;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.Linq;
using MimeKit;
using System.Threading.Tasks;
using Rnwood.SmtpServer;
using System.Net.Sockets;
using MailKit.Security;
using MiddleMail.Delivery.Smtp;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;

namespace MiddleMail.Tests.Delivery {

	public class SmtpMimeMessageSenderTests : IDisposable {

		private readonly SmtpMimeMessageSender smtpSender;

		private readonly SmtpOptions options;

		private DefaultServer smtpServer;

		private AuthenticationResult? authenticationResult;

		List<IMessage> messages;

		public SmtpMimeMessageSenderTests() {
			options = new SmtpOptions {
				Server = "localhost",
				Port = 50000,
				Username = "username",
				Password = "pa$$word",
			};

			var logger = new NullLogger<SmtpMimeMessageSender>();
			smtpSender = new SmtpMimeMessageSender(Options.Create(options), logger);
			startServer();
		}

		private void startServer() {
			smtpServer = new DefaultServer(false, options.Port);
			messages = new List<IMessage>();
			smtpServer.MessageReceivedEventHandler += (o, ea) => {
				messages.Add(ea.Message);
				return Task.CompletedTask;
			};

			smtpServer.AuthenticationCredentialsValidationRequiredEventHandler += (o, ea) => {
				ea.AuthenticationResult = this.authenticationResult ?? AuthenticationResult.Success;
				return Task.CompletedTask;
			};

			smtpServer.Start();
		}

		public void Dispose() {
			smtpServer.Dispose();
			smtpSender.Dispose();
		}

		[Fact]
		public async void SendSingleEmail() {
			var mimeMessage = await sendRandomEmail();
			Assert.Single(messages);
			
			Assert.Equal(((MailboxAddress) mimeMessage.From.First()).Address, messages.First().From);
		}

		[Fact]
		public async void Send100Emails() {
			for(int i = 1; i <= 100; i++) {
				await sendRandomEmail();
				Assert.Equal(i, messages.Count);
			}
		}

		/// <summary>
		/// <see cref="MailKit.Net.Smtp.SmtpClient" /> is not thread safe. The <see cref="System.Threading.SemaphoreSlim" />
		/// in <see cref="SmtpMimeMessageSender" /> synchronizes access to the SmtpClient instance. Without the semaphore
		/// this test fails.
		/// </summary>
		[Fact]
		public async void Send100EmailsMultiThreaded() {
			var task1 = Task.Run( async () => {
				for(int i = 1; i <= 100; i++) {
					await sendRandomEmail();
				}
			});
			var task2 = Task.Run( async () => {
				for(int i = 1; i <= 100; i++) {
					await sendRandomEmail();
				}
			});
			await task1;
			await task2;
			Assert.Equal(200, messages.Count);
		}
		

		[Fact]
		public async void SendEmailAfterServerDisconnect() {
			await sendRandomEmail();
			Assert.Single(messages);

			// restart the server, this forces the connection to reconnect
			smtpServer.Stop();
			startServer();

			await sendRandomEmail();
			Assert.Single(messages);
		}


		[Fact]
		public async void ThrowsIfServerOffline() {
			smtpServer.Stop();
			await Assert.ThrowsAnyAsync<SocketException>(sendRandomEmail);
			startServer();
			await sendRandomEmail();
			Assert.Single(messages);
			smtpServer.Stop();
			await Assert.ThrowsAnyAsync<SocketException>(sendRandomEmail);
		}

		
		[Fact]
		public async void SendInvalidEmail() {
			await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await sendEmail(new MimeMessage()));
		}

		[Fact]
		public async void ThrowsIfAuthenticationFailure() {
			this.authenticationResult = AuthenticationResult.Failure;
			await Assert.ThrowsAnyAsync<AuthenticationException>(sendRandomEmail);

			// test if reauthentication works
			this.authenticationResult = AuthenticationResult.Success;
			await sendRandomEmail();
		}
		
		private async Task<MimeMessage> sendRandomEmail() {
			var message = FakerFactory.MimeMessageFaker.Generate();
			await sendEmail(message);
			return message;
		}

		private async Task sendEmail(MimeMessage message) {
			await smtpSender.SendAsync(message);
			// delay here, allowing the server some time to handle the message
			await Task.Delay(20);
		}
	}
}
