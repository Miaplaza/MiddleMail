using System.Threading;
using System;
using System.Threading.Tasks;
using MiaPlaza.MiddleMail.Delivery;
using MiaPlaza.MiddleMail.Model;
using MiaPlaza.MiddleMail.Storage;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using MiaPlaza.MiddleMail.Exceptions;
using System.Text;

namespace MiaPlaza.MiddleMail.Tests {
	public class MessageProcessorTests {

		private const string DELIVER_FAILURE = "DELIVER_FAILURE";
		private const string SET_PROCESSED_FAILURE = "SET_PROCESSED_FAILURE";
		private const string SET_ERROR_FAILURE = "SET_ERROR_FAILURE";
		private const string SET_SENT_FAILURE = "SET_SENT_FAILURE";
		private readonly Guid CACHE_GET_EXCEPTION = Guid.NewGuid();

		private readonly Guid CACHE_SET_EXCEPTION = Guid.NewGuid();


		private readonly Mock<IMailDeliverer> delivererMock;
		private readonly Mock<IMailStorage> storageMock;
		private readonly MessageProcessor messageProcessor;

		private readonly Mock<IDistributedCache> cacheMock;

		public MessageProcessorTests() {
			delivererMock = new Mock<IMailDeliverer>();

			delivererMock
				.Setup(d => d.DeliverAsync(It.Is<EmailMessage>(m => m.Subject == DELIVER_FAILURE)))
				.ThrowsAsync(new Exception());
			
			storageMock = new Mock<IMailStorage>();

			storageMock
				.Setup(s => s.SetProcessedAsync(It.Is<EmailMessage>(m => m.Subject == SET_PROCESSED_FAILURE)))
				.ThrowsAsync(new Exception());
			
			storageMock
				.Setup(s => s.SetErrorAsync(It.Is<EmailMessage>(m => m.Subject == SET_ERROR_FAILURE), It.IsAny<string>()))
				.ThrowsAsync(new Exception());

			storageMock
				.Setup(s => s.SetSentAsync(It.Is<EmailMessage>(m => m.Subject == SET_SENT_FAILURE)))
				.ThrowsAsync(new Exception());

			cacheMock = new Mock<IDistributedCache>();

			var internalCache = new MemoryDistributedCache(Options.Create<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));

			cacheMock
				.Setup(c => c.GetAsync(It.Is<string>(m => m == CACHE_GET_EXCEPTION.ToString()), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Exception());

			cacheMock
				.Setup(c => c.GetAsync(It.Is<string>(m => m != CACHE_GET_EXCEPTION.ToString()), It.IsAny<CancellationToken>()))
				.Returns(async (string key, CancellationToken c) => await internalCache.GetAsync(key, c));

			cacheMock
				.Setup(c => c.SetAsync(It.Is<string>(m => m == CACHE_SET_EXCEPTION.ToString()), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Exception());

			cacheMock
				.Setup(c => c.SetAsync(It.Is<string>(m => m != CACHE_SET_EXCEPTION.ToString()), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
				.Returns(async (string key, byte[] value, DistributedCacheEntryOptions o, CancellationToken c) => await internalCache.SetAsync(key, value, o, c));

			var logger = new NullLogger<MessageProcessor>();	
			messageProcessor = new MessageProcessor(delivererMock.Object, storageMock.Object, cacheMock.Object, logger);
		}

		private async Task<EmailMessage> processValidMessage() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			await messageProcessor.ProcessAsync(emailMessage);
			return emailMessage;
		}

		[Fact]
		public async Task SuccessfullProcessingDelivers() {
			await processValidMessage();
			delivererMock.Verify(d => d.DeliverAsync(It.IsAny<EmailMessage>()), Times.Once);
			storageMock.Verify(d => d.SetProcessedAsync(It.IsAny<EmailMessage>()), Times.Once);
			storageMock.Verify(d => d.SetErrorAsync(It.IsAny<EmailMessage>(), It.IsAny<string>()), Times.Never);
			storageMock.Verify(d => d.SetSentAsync(It.IsAny<EmailMessage>()), Times.Once);
		}

		[Fact]
		public async Task SuccessfullProcessingStores() {
			await processValidMessage();
			storageMock.Verify(d => d.SetProcessedAsync(It.IsAny<EmailMessage>()), Times.Once);
			storageMock.Verify(d => d.SetErrorAsync(It.IsAny<EmailMessage>(), It.IsAny<string>()), Times.Never);
			storageMock.Verify(d => d.SetSentAsync(It.IsAny<EmailMessage>()), Times.Once);
		}

		[Fact]
		public async Task SuccessfullDeliveryCaches() {
			var emailMessage = await processValidMessage();
			Assert.NotNull(await cacheMock.Object.GetStringAsync(emailMessage.Id.ToString()));
		}

		[Fact]
		public async Task UnsuccessfulDeliveryDoesNotCache() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Subject = DELIVER_FAILURE;
			try { await messageProcessor.ProcessAsync(emailMessage); } catch { }
			Assert.Null(await cacheMock.Object.GetStringAsync(emailMessage.Id.ToString()));
		}

		[Fact]
		public async Task StorageExceptionInSetProcessedDoesNotThrow() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Subject = SET_PROCESSED_FAILURE;
			await messageProcessor.ProcessAsync(emailMessage);
			delivererMock.Verify(d => d.DeliverAsync(It.IsAny<EmailMessage>()), Times.Once);
		}

		[Fact]
		public async Task StorageExceptionInSetSentDoesNotThrow() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Subject = SET_SENT_FAILURE;
			await messageProcessor.ProcessAsync(emailMessage);
			delivererMock.Verify(d => d.DeliverAsync(It.IsAny<EmailMessage>()), Times.Once);
		}

		[Fact]
		public async void StorageExceptionInSetErrorDoesNotThrow() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Subject = SET_ERROR_FAILURE;
			await messageProcessor.ProcessAsync(emailMessage);
			delivererMock.Verify(d => d.DeliverAsync(It.IsAny<EmailMessage>()), Times.Once);
		}

		[Fact]
		public async Task UnsuccessfulDeliveryStores() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Subject = DELIVER_FAILURE;
			await Assert.ThrowsAnyAsync<Exception>(async () => await messageProcessor.ProcessAsync(emailMessage));
			delivererMock.Verify(d => d.DeliverAsync(It.IsAny<EmailMessage>()), Times.Once);
			storageMock.Verify(d => d.SetProcessedAsync(It.IsAny<EmailMessage>()), Times.Once);
			storageMock.Verify(d => d.SetErrorAsync(It.IsAny<EmailMessage>(), It.IsAny<string>()), Times.Once);
			storageMock.Verify(d => d.SetSentAsync(It.IsAny<EmailMessage>()), Times.Never);
		}

		[Fact]
		public async Task ExceptionInCacheThrowsGeneralExceptionBeforeDelivery() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Id = CACHE_GET_EXCEPTION;
			await Assert.ThrowsAnyAsync<GeneralProcessingException>(async () => await messageProcessor.ProcessAsync(emailMessage));
		}

		[Fact]
		public async Task ExceptionInCacheThrowsExceptionAfterDelivery() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Id = CACHE_SET_EXCEPTION;
			await Assert.ThrowsAsync<Exception>(async () => await messageProcessor.ProcessAsync(emailMessage));
		}

		[Fact]
		public async Task EmailMessageIsNotStoredWhenStorePropertyIsFalse() {
			var emailMessage = FakerFactory.EmailMessageFaker.Generate();
			emailMessage.Store = false;
			await messageProcessor.ProcessAsync(emailMessage);
			storageMock.Verify(d => d.SetProcessedAsync(It.IsAny<EmailMessage>()), Times.Never);
			storageMock.Verify(d => d.SetErrorAsync(It.IsAny<EmailMessage>(), It.IsAny<string>()), Times.Never);
			storageMock.Verify(d => d.SetSentAsync(It.IsAny<EmailMessage>()), Times.Never);	
		}
	}
}
