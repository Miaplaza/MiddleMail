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
using MimeKit.Text;
using MiddleMail.Model;
using MiddleMail.Delivery.Smtp;
using Microsoft.Extensions.Options;

namespace MiddleMail.Tests.Delivery {

	public class MimeMessageBuilderTests : IDisposable {

		private const int SMTP_PORT = 50001;

		private readonly MimeMessageBuilder builder;


		private DefaultServer smtpServer;
		private readonly List<IMessage> messages;

		private readonly string MessageIdDomainPart = "testdomain.test";

		public MimeMessageBuilderTests() {
			var options = new MimeMessageOptions {
				MessageIdDomainPart = MessageIdDomainPart
			};

			builder = new MimeMessageBuilder(Options.Create(options));
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
				
				await Task.Delay(50);
				Assert.Single(messages);
				
				var parser = new MimeParser(await messages.First().GetData(), MimeFormat.Entity);
				var parsedMessage = parser.ParseMessage();

				Assert.Equal(emailMessage.Subject, parsedMessage.Subject);

				Assert.Equal(emailMessage.From.name ?? string.Empty, parsedMessage.From.First().Name);
				Assert.Equal(emailMessage.From.address, ((MailboxAddress)parsedMessage.From.First()).Address);

				Assert.Equal(emailMessage.To.name ?? string.Empty, parsedMessage.To.First().Name);
				Assert.Equal(emailMessage.To.address, ((MailboxAddress)parsedMessage.To.First()).Address);

				Assert.Equal(emailMessage.Cc.Count, parsedMessage.Cc.Count);
				for(int i = 0; i < emailMessage.Cc.Count; i++) {
					Assert.Equal(emailMessage.Cc[i].name ?? string.Empty, parsedMessage.Cc[i].Name);
					Assert.Equal(emailMessage.Cc[i].address, ((MailboxAddress)parsedMessage.Cc[i]).Address);
				}

				if (!emailMessage.ReplyTo.HasValue) {
					Assert.Empty(parsedMessage.ReplyTo);
				} else {
					Assert.Equal(emailMessage.ReplyTo.Value.name ?? string.Empty, parsedMessage.ReplyTo.First().Name);
					Assert.Equal(emailMessage.ReplyTo.Value.address, ((MailboxAddress)parsedMessage.ReplyTo.First()).Address);
				}	
				
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
			emailMessage.From = (null, "<>");

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

		[Fact]
		public void BuildSubjectNullThrows() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Subject = null;
			
			Assert.ThrowsAny<Exception>(() => builder.Create(emailMessage));
		}

		[Fact]
		public void BuildFromEmailNullThrows() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.From = (emailMessage.From.name, null);
			
			Assert.ThrowsAny<Exception>(() => builder.Create(emailMessage));
		}

		[Fact]
		public void BuildToEmailNullThrows() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.To = (emailMessage.To.name, null);
			
			Assert.ThrowsAny<Exception>(() => builder.Create(emailMessage));
		}

		[Fact]
		public void MessageIdLocalPartIsEmailMessageId() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			var mimeMessage = builder.Create(emailMessage);

			Assert.StartsWith(emailMessage.Id.ToString("N"), mimeMessage.MessageId);
		}

		[Fact]
		public void MessageIdDomainPartIsSet() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			var mimeMessage = builder.Create(emailMessage);

			Assert.EndsWith(MessageIdDomainPart, mimeMessage.MessageId);
		}

		[Fact]
		public async Task ReplyToNotSetIfNotSpecified() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.ReplyTo = null;
			var mimeMessage = builder.Create(emailMessage);

			await sendEmailAndAssertEquality(emailMessage, mimeMessage);
		}

		[Fact]
		public async Task NamesCanBeNull() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.ReplyTo = (null, emailMessage.ReplyTo.Value.address);
			emailMessage.To = (null, emailMessage.To.address);
			emailMessage.From = (null, emailMessage.From.address);

			var mimeMessage = builder.Create(emailMessage);

			await sendEmailAndAssertEquality(emailMessage, mimeMessage);
		}

		[Fact]
		public async Task HeadersAreRendered() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Headers.Add("X-MiddleMail", "test");
			emailMessage.Headers.Add("Precedence", "bulk");
			
			var mimeMessage = builder.Create(emailMessage);
			Assert.Equal("test", mimeMessage.Headers["X-MiddleMail"]);
			Assert.Equal("bulk", mimeMessage.Headers["Precedence"]);

			await sendEmailAndAssertEquality(emailMessage, mimeMessage);
		}

		public void Dispose() {
			smtpServer.Dispose(); 
		}
	}
}
