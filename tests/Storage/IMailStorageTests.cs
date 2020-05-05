using System;
using MiaPlaza.MiddleMail.Exceptions;
using MiaPlaza.MiddleMail.Model;
using MiaPlaza.MiddleMail.Storage;
using Xunit;


namespace MiaPlaza.MiddleMail.Tests.Storage {
	public abstract class IMailStorageTests<T> where T : IMailStorage {

		protected abstract T MailStorage { get; }

		private EmailMessage emailMessage = FakerFactory.EmailMessageFaker.Generate();

		[Fact]
		public async void MessageProcessed() {
			await MailStorage.SetProcessedAsync(emailMessage);
			Assert.True(await MailStorage.TryGetProcessedCount(emailMessage, out var processedCount));
			Assert.True(await MailStorage.TryGetSent(emailMessage, out var sent));
			Assert.True(await MailStorage.TryGetError(emailMessage, out var error));

			Assert.Equal(1, processedCount);
			Assert.False(sent);
			Assert.Null(error);
		}

		[Fact]
		public async void MessageprocessedCountIncreases() {
			await MailStorage.SetProcessedAsync(emailMessage);
			Assert.True(await MailStorage.TryGetProcessedCount(emailMessage, out var processedCount1));
			Assert.Equal(1, processedCount1);

			await MailStorage.SetProcessedAsync(emailMessage);
			Assert.True(await MailStorage.TryGetProcessedCount(emailMessage, out var processedCount2));
			Assert.Equal(2, processedCount2);

			await MailStorage.SetProcessedAsync(emailMessage);
			Assert.True(await MailStorage.TryGetProcessedCount(emailMessage, out var processedCount3));
			Assert.Equal(3, processedCount3);
		}

		[Fact]
		public async void MessageprocessedCountDoesNotDecrease() { 
			await MailStorage.SetProcessedAsync(emailMessage);
			await MailStorage.SetProcessedAsync(emailMessage);
			await MailStorage.SetProcessedAsync(emailMessage);
			await MailStorage.SetProcessedAsync(emailMessage);

			Assert.True(await MailStorage.TryGetProcessedCount(emailMessage, out var processedCount));
			Assert.Equal(4, processedCount);

			await MailStorage.SetErrorAsync(emailMessage, "error message");
			Assert.True(await MailStorage.TryGetProcessedCount(emailMessage, out var processedCount2));
			Assert.Equal(4, processedCount2);

			await MailStorage.SetSentAsync(emailMessage);
			Assert.True(await MailStorage.TryGetProcessedCount(emailMessage, out var processedCount3));
			Assert.Equal(4, processedCount3);
		}

		[Fact]
		public async void ErrorIsStoredAndOverwritten() {
			var errorMessage = "error message";

			await MailStorage.SetProcessedAsync(emailMessage);
			await MailStorage.SetErrorAsync(emailMessage, errorMessage);
			
			Assert.True(await MailStorage.TryGetError(emailMessage, out var error));
			Assert.Equal(errorMessage, error);

			var errorMessage2 = "error message 2";

			await MailStorage.SetErrorAsync(emailMessage, errorMessage2);
			Assert.True(await MailStorage.TryGetError(emailMessage, out var error2));
			Assert.Equal(errorMessage2, error2);
		}

		[Fact]
		public async void SentIsStored() {
			await MailStorage.SetProcessedAsync(emailMessage);
			await MailStorage.SetSentAsync(emailMessage);

			Assert.True(await MailStorage.TryGetSent(emailMessage, out var sent));
			Assert.True(sent);
		}

		[Fact]
		public async void TryGetForNotProcessedMessage() {
			Assert.False(await MailStorage.TryGetProcessedCount(emailMessage, out var processedCount));
			Assert.False(await MailStorage.TryGetError(emailMessage, out var error));			
			Assert.False(await MailStorage.TryGetError(emailMessage, out var sent));
		}
		
		[Fact]
		public async void SetForNotProcessedMessageThrows() {
			await Assert.ThrowsAsync<EMailMessageNotFoundInStorageException>(async () => await MailStorage.SetErrorAsync(emailMessage, "error message"));
			await Assert.ThrowsAsync<EMailMessageNotFoundInStorageException>(async () => await MailStorage.SetSentAsync(emailMessage));
		}

		[Fact]
		public async void SetForAlreadySentMessageThrows() {
			await MailStorage.SetProcessedAsync(emailMessage);
			await MailStorage.SetSentAsync(emailMessage);

			await Assert.ThrowsAsync<EMailMessageAlreadySentStorageException>(async () => await MailStorage.SetErrorAsync(emailMessage, "error message"));
			await Assert.ThrowsAsync<EMailMessageAlreadySentStorageException>(async () => await MailStorage.SetSentAsync(emailMessage));
			await Assert.ThrowsAsync<EMailMessageAlreadySentStorageException>(async () => await MailStorage.SetProcessedAsync(emailMessage));
		}
	}
}
