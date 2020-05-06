using System.Threading.Tasks;
using System;
using MiaPlaza.MiddleMail.Exceptions;
using MiaPlaza.MiddleMail.Model;
using MiaPlaza.MiddleMail.Storage;
using Xunit;


namespace MiaPlaza.MiddleMail.Tests.Storage {
	public abstract class IMailStorageTests  {

		protected abstract IMailStorage MailStorage { get; }

		private EmailMessage emailMessage = FakerFactory.EmailMessageFaker.Generate();

		[Fact]
		public async void MessageProcessed() {
			await MailStorage.SetProcessedAsync(emailMessage);
			var sent = await MailStorage.GetSentAsync(emailMessage);
			var error = await MailStorage.GetErrorAsync(emailMessage);

			Assert.False(sent);
			Assert.Null(error);
		}

		[Fact]
		public async void ErrorIsStoredAndOverwritten() {
			var errorMessage = "error message";

			await MailStorage.SetProcessedAsync(emailMessage);
			await MailStorage.SetErrorAsync(emailMessage, errorMessage);
			
			var error = await MailStorage.GetErrorAsync(emailMessage);
			Assert.Equal(errorMessage, error);

			var errorMessage2 = "error message 2";

			await MailStorage.SetErrorAsync(emailMessage, errorMessage2);
			var error2 = await MailStorage.GetErrorAsync(emailMessage);
			Assert.Equal(errorMessage2, error2);
		}

		[Fact]
		public async void SentIsStored() {
			await MailStorage.SetProcessedAsync(emailMessage);
			await MailStorage.SetSentAsync(emailMessage);
			Assert.True(await MailStorage.GetSentAsync(emailMessage));
		}

		[Fact]
		public async void TryGetForNotProcessedMessage() {
			Assert.Null(await MailStorage.GetErrorAsync(emailMessage));			
			Assert.Null(await MailStorage.GetErrorAsync(emailMessage));
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
