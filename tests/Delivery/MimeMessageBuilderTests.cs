using System;
using System.Net;
using System.Collections.Generic;
using MiaPlaza.MailService.Delivery;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.Linq;
using MimeKit;
using System.Threading.Tasks;
using Rnwood.SmtpServer;
using System.Net.Sockets;
using MailKit.Security;
using MimeKit.Text;

namespace MiaPlaza.MailService.Tests.Delivery {

	public class MimeMessageBuilderTests : IDisposable {

		private const int SMTP_PORT = 50001;

		private readonly MimeMessageBuilder builder;


		private DefaultServer smtpServer;
		private readonly List<IMessage> messages;

		public MimeMessageBuilderTests() {
			builder = new MimeMessageBuilder();
			messages = new List<IMessage>();
			smtpServer = new DefaultServer(false, SMTP_PORT);
			smtpServer.MessageReceivedEventHandler += (o, ea) => {
				messages.Add(ea.Message);
				return Task.CompletedTask;
			};
			smtpServer.Start();
		}

		private async Task sendEmailAndAssertEquality(EmailMessage emailMessage, MimeMessage mimeMessage) {
			using(var smtpClient = new MailKit.Net.Smtp.SmtpClient()) {
				await smtpClient.ConnectAsync("localhost", SMTP_PORT);
				await smtpClient.SendAsync(mimeMessage);
				
				Assert.Single(messages);
				
				var parser = new MimeParser(await messages.First().GetData(), MimeFormat.Entity);
				var parsedMessage = parser.ParseMessage();

				Assert.Equal(emailMessage.Subject, parsedMessage.Subject);
				Assert.Equal(emailMessage.FromName, parsedMessage.From.First().Name);
				Assert.Equal(emailMessage.FromEmail, ((MailboxAddress)parsedMessage.From.First()).Address);
				Assert.Equal(emailMessage.ToName, parsedMessage.To.First().Name);
				Assert.Equal(emailMessage.ToEmail, ((MailboxAddress)parsedMessage.To.First()).Address);
				Assert.Equal(emailMessage.PlainText, parsedMessage.GetTextBody(TextFormat.Text));
				Assert.Equal(emailMessage.HtmlText, parsedMessage.HtmlBody);
			}
		}

		[Fact]
		public async void BuilValidEmailMessage() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			var mimeMessage = builder.Create(emailMessage);

			await sendEmailAndAssertEquality(emailMessage, mimeMessage);
		}

		[Fact]
		public void BuilInvalidEmailMessageThrows() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.FromEmail = "<>";

			Assert.ThrowsAny<Exception>(() => builder.Create(emailMessage));
		}

		[Fact]
		public void BuildPlainAndHtmlNullThrows() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.PlainText = null;
			emailMessage.HtmlText = null;

			Assert.Throws<ArgumentException>(() => builder.Create(emailMessage));
		}

		[Fact]
		public void BuildPlainNullThrows() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.PlainText = null;

			Assert.Throws<ArgumentException>(() => builder.Create(emailMessage));
		}

		[Fact]
		public async void BuildHtmlNull() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.HtmlText = null;
			var mimeMessage = builder.Create(emailMessage);
			await sendEmailAndAssertEquality(emailMessage, mimeMessage);
		}

		public void Dispose() {
			smtpServer.Dispose();
		}
	}
}
