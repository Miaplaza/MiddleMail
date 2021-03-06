using System;
using MiddleMail.Delivery;
using Xunit;
using MimeKit;
using Moq;
using MiddleMail.Exceptions;
using MiddleMail.Model;
using MiddleMail.Delivery.Smtp;

namespace MiddleMail.Tests.Delivery {

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
				.ThrowsAsync(new InvalidOperationException());
			
			messageSenderMock
				.Setup(s => s.SendAsync(It.Is<MimeMessage>(m => m.Subject == GENERAL_SMTP_ERROR)))
				.ThrowsAsync(new Exception());

			smtpDeliverer = new SmtpDeliverer(messageBuilderMock.Object, messageSenderMock.Object);
		}

		[Fact]
		public async void ThrowsIfBuildFails() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Subject = BUILDER_FAILURE;
			await Assert.ThrowsAnyAsync<SingleProcessingException>(async () => await smtpDeliverer.DeliverAsync(emailMessage));
			messageSenderMock.Verify(s => s.SendAsync(It.IsAny<MimeMessage>()), Times.Never);
		}

		[Fact (Skip = "Disabled because of workaround in 4cbb493b23b628d262dc6a2429d4e88eaa5d673c")]
		public async void ThrowsIfInvalidSendFails() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Subject = MESSAGE_INVALID;

			await Assert.ThrowsAnyAsync<SingleProcessingException>(async () => await smtpDeliverer.DeliverAsync(emailMessage));
			messageSenderMock.Verify(s => s.SendAsync(It.IsAny<MimeMessage>()), Times.Once);
		}

		[Fact]
		public async void ThrowsIfSmtpFails() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Subject = GENERAL_SMTP_ERROR;

			await Assert.ThrowsAnyAsync<GeneralProcessingException>(async () => await smtpDeliverer.DeliverAsync(emailMessage));
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
