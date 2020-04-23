using System;
using MiaPlaza.MailService.Delivery;
using Xunit;
using MimeKit;
using Moq;
using MiaPlaza.MailService.Exceptions;

namespace MiaPlaza.MailService.Tests.Delivery {

	public class SmtpDelivererTests  {

		private const string BUILDER_FAILURE = "builder_failure";
		private const string MESSAGE_INVALID = "message_invalid";

		private const string GENERAL_SMTP_ERROR = "smtp_error";
		
		private readonly SmtpDeliverer smtpDeliverer;
		private readonly Mock<IMimeMessageBuilder> messageBuilderMock;
		private readonly Mock<IMimeMessageSender> messageSenderMock;

		public SmtpDelivererTests() {
			messageBuilderMock = new Mock<IMimeMessageBuilder>();
			messageBuilderMock
				.Setup(b => b.Create(It.Is<EmailMessage>(m => m.Subject != BUILDER_FAILURE)))
				.Returns<EmailMessage>((emailMessage) => {
					var mimeMessage = FakerFactory.MimeMessageFaker.Generate();
					mimeMessage.Subject = emailMessage.Subject;
					return mimeMessage;
				});

			messageBuilderMock
				.Setup(b => b.Create(It.Is<EmailMessage>(m => m.Subject == BUILDER_FAILURE)))
				.Throws(new Exception());

			messageSenderMock = new Mock<IMimeMessageSender>();
			messageSenderMock
				.Setup(s => s.SendAsync(It.Is<MimeMessage>(m => m.Subject == MESSAGE_INVALID)))
				.Throws(new InvalidOperationException());
			
			messageSenderMock
				.Setup(s => s.SendAsync(It.Is<MimeMessage>(m => m.Subject == GENERAL_SMTP_ERROR)))
				.Throws(new Exception());

			smtpDeliverer = new SmtpDeliverer(messageBuilderMock.Object, messageSenderMock.Object);
		}

		[Fact]
		public async void ThrowsIfBuildFails() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Subject = BUILDER_FAILURE;
			await Assert.ThrowsAnyAsync<SingleDeliveryException>(async () => await smtpDeliverer.DeliverAsync(emailMessage));
			messageSenderMock.Verify(s => s.SendAsync(It.IsAny<MimeMessage>()), Times.Never);
		}

		[Fact]
		public async void ThrowsIfInvalidSendFails() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Subject = MESSAGE_INVALID;

			await Assert.ThrowsAnyAsync<SingleDeliveryException>(async () => await smtpDeliverer.DeliverAsync(emailMessage));
			messageSenderMock.Verify(s => s.SendAsync(It.IsAny<MimeMessage>()), Times.Once);
		}

		[Fact]
		public async void ThrowsIfSmtpFails() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Subject = GENERAL_SMTP_ERROR;

			await Assert.ThrowsAnyAsync<GlobalDeliveryException>(async () => await smtpDeliverer.DeliverAsync(emailMessage));
			messageSenderMock.Verify(s => s.SendAsync(It.IsAny<MimeMessage>()), Times.Once);
		}

		[Fact]
		public async void TestEmailDelivery() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();

			await smtpDeliverer.DeliverAsync(emailMessage);
			messageBuilderMock.Verify(s => s.Create(It.IsAny<EmailMessage>()), Times.Once);
			messageSenderMock.Verify(s => s.SendAsync(It.IsAny<MimeMessage>()), Times.Once);
		}
	}
}
