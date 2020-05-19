using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiaPlaza.MiddleMail.Model;
using MiaPlaza.MiddleMail.Storage.ElasticSearch;
using Microsoft.Extensions.Configuration;
using Moq;

namespace MiaPlaza.MiddleMail.Tests.Storage {
	public class ElasticSearchStorageTests : IMailStorageTests {
		
		private readonly ElasticSearchStorage elasticSearchStorage;
		private readonly Mock<IMailStorage> elasticSearchStorageWithDelay;

		private string host = Environment.GetEnvironmentVariable("ElasticSearch__Host") ?? "localhost";

		public ElasticSearchStorageTests() {
			var config = new Dictionary<string, string>{
				{"ElasticSearchStorage:Uri", $"http://{host}:9200"},
				{"ElasticSearchStorage:Index", "middlemail-test"}
			};

			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(config)
				.Build();

			// after writes to elasticsearch it takes some time that they are reflected in search results
			// we simply wait 1 second after each write operation and before each read.
			// The IMailStorage implementation should however always be robust enough to not rely on the underlying storage to serve up-to-date data.
			this.elasticSearchStorage = new ElasticSearchStorage(new ElasticSearchStorageConfiguration(configuration));
			elasticSearchStorageWithDelay = new Mock<IMailStorage>();

			elasticSearchStorageWithDelay
				.Setup(s => s.SetProcessedAsync(It.IsAny<EmailMessage>()))
				.Returns(async (EmailMessage emailMessage) => {
					await elasticSearchStorage.SetProcessedAsync(emailMessage);
					await Task.Delay(1000);
				});

			elasticSearchStorageWithDelay
				.Setup(s => s.SetErrorAsync(It.IsAny<EmailMessage>(), It.IsAny<string>()))
				.Returns(async (EmailMessage emailMessage, string error) => {
					await elasticSearchStorage.SetErrorAsync(emailMessage, error);
					await Task.Delay(1000);
				});

			elasticSearchStorageWithDelay
				.Setup(s => s.SetSentAsync(It.IsAny<EmailMessage>()))
				.Returns(async (EmailMessage emailMessage) => {
					await elasticSearchStorage.SetSentAsync(emailMessage);
					await Task.Delay(1000);
				});

			elasticSearchStorageWithDelay
				.Setup(s => s.GetErrorAsync(It.IsAny<EmailMessage>()))
				.Returns(async (EmailMessage emailMessage) => {
					await Task.Delay(1000);
					var error = await elasticSearchStorage.GetErrorAsync(emailMessage);
					return error;
				});
			
			elasticSearchStorageWithDelay
				.Setup(s => s.GetSentAsync(It.IsAny<EmailMessage>()))
				.Returns(async (EmailMessage emailMessage) => {
					await Task.Delay(1000);
					var sent = await elasticSearchStorage.GetSentAsync(emailMessage);
					return sent;
				});

		}
		protected override IMailStorage MailStorage => elasticSearchStorageWithDelay.Object;
	}
}
