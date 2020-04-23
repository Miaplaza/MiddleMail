using System;
using MiaPlaza.MailService.Exceptions;
using MiaPlaza.MailService.Storage;
using Xunit;


namespace MiaPlaza.MailService.Tests.Storage {
	public abstract class IMailStorageTests<T> where T : IMailStorage {

		protected abstract T MailStorage { get; }

		private EmailMessage emailMessage = FakerFactory.EmailMessageFaker.Generate();

		[Fact]
		public async void MessageHandled() {
			await MailStorage.SetHandledAsync(emailMessage);
			Assert.True(await MailStorage.TryGetHandleCount(emailMessage, out var handleCount));
			Assert.True(await MailStorage.TryGetSent(emailMessage, out var sent));
			Assert.True(await MailStorage.TryGetError(emailMessage, out var error));

			Assert.Equal(1, handleCount);
			Assert.False(sent);
			Assert.Null(error);
		}

		[Fact]
		public async void MessageHandleCountIncreases() {
			await MailStorage.SetHandledAsync(emailMessage);
			Assert.True(await MailStorage.TryGetHandleCount(emailMessage, out var handleCount1));
			Assert.Equal(1, handleCount1);

			await MailStorage.SetHandledAsync(emailMessage);
			Assert.True(await MailStorage.TryGetHandleCount(emailMessage, out var handleCount2));
			Assert.Equal(2, handleCount2);

			await MailStorage.SetHandledAsync(emailMessage);
			Assert.True(await MailStorage.TryGetHandleCount(emailMessage, out var handleCount3));
			Assert.Equal(3, handleCount3);
		}

		[Fact]
		public async void MessageHandleCountDoesNotDecrease() { 
			await MailStorage.SetHandledAsync(emailMessage);
			await MailStorage.SetHandledAsync(emailMessage);
			await MailStorage.SetHandledAsync(emailMessage);
			await MailStorage.SetHandledAsync(emailMessage);

			Assert.True(await MailStorage.TryGetHandleCount(emailMessage, out var handleCount));
			Assert.Equal(4, handleCount);

			await MailStorage.SetErrorAsync(emailMessage, "error message");
			Assert.True(await MailStorage.TryGetHandleCount(emailMessage, out var handleCount2));
			Assert.Equal(4, handleCount2);

			await MailStorage.SetSentAsync(emailMessage);
			Assert.True(await MailStorage.TryGetHandleCount(emailMessage, out var handleCount3));
			Assert.Equal(4, handleCount3);
		}

		[Fact]
		public async void ErrorIsStoredAndOverwritten() {
			var errorMessage = "error message";

			await MailStorage.SetHandledAsync(emailMessage);
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
			await MailStorage.SetHandledAsync(emailMessage);
			await MailStorage.SetSentAsync(emailMessage);

			Assert.True(await MailStorage.TryGetSent(emailMessage, out var sent));
			Assert.True(sent);
		}

		[Fact]
		public async void TryGetInfoOfNotHandledMessage() {
			Assert.False(await MailStorage.TryGetHandleCount(emailMessage, out var handleCount));
			Assert.False(await MailStorage.TryGetError(emailMessage, out var error));			
			Assert.False(await MailStorage.TryGetError(emailMessage, out var sent));
		}
		
		[Fact]
		public async void SetInfoOfNotHandledMessageThrows() {
			await Assert.ThrowsAsync<EMailMessageNotFoundInStorageException>(async () => await MailStorage.SetErrorAsync(emailMessage, "error message"));
			await Assert.ThrowsAsync<EMailMessageNotFoundInStorageException>(async () => await MailStorage.SetSentAsync(emailMessage));
		}

		[Fact]
		public async void HandleAlreadySentMessageThrows() {
			await MailStorage.SetHandledAsync(emailMessage);
			await MailStorage.SetSentAsync(emailMessage);

			await Assert.ThrowsAsync<EMailMessageAlreadySentStorageException>(async () => await MailStorage.SetErrorAsync(emailMessage, "error message"));
			await Assert.ThrowsAsync<EMailMessageAlreadySentStorageException>(async () => await MailStorage.SetSentAsync(emailMessage));
			await Assert.ThrowsAsync<EMailMessageAlreadySentStorageException>(async () => await MailStorage.SetHandledAsync(emailMessage));
		}
	}
}
