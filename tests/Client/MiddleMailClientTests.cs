using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiddleMail.Client.RabbitMQ;
using Moq;
using Xunit;

namespace MiddleMail.Tests.Client {

	[Collection("RabbitMQ Test Collection")]
	public class MiddleMailClientTests {

		private readonly Mock<ILogger<MiddleMailClient>> loggerMock;

		private const string UNREACHABLE_CONNECTIONSTRING = "host=unreachable";

		public MiddleMailClientTests() {
			loggerMock = new Mock<ILogger<MiddleMailClient>>();
		}

		private MiddleMailClient createMiddleMailClient(string connectionString) {
			var options = Options.Create(new RabbitMQOptions { ConnectionString = connectionString });
			return new MiddleMailClient(options, loggerMock.Object);
		}

		[Fact]
		public async Task SendEmailAsync_ShouldThrowAndLogException_WhenRabbitMQBusThrowsException() {
			// Arrange
			var middleMailClient = createMiddleMailClient(UNREACHABLE_CONNECTIONSTRING);
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();

			// Act / Assert
			await Assert.ThrowsAnyAsync<Exception>(() => middleMailClient.SendEmailAsync(emailMessage));
			loggerMock.VerifyLog(m => m.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);

		}

		[Fact]
		public async Task SendEmailAsync_ShouldNotThrowNotLogException_WhenMessageSuccessfullyPublished() {
			// Arrange
			var middleMailClient = createMiddleMailClient(RabbitMQTestHelpers.ConnectionString);
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();

			// Act
			await middleMailClient.SendEmailAsync(emailMessage);

			// Assert
			loggerMock.VerifyLog(m => m.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
		}
	}
}
