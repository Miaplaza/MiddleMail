using MiaPlaza.MailService.Storage;

namespace MiaPlaza.MailService.Tests.Storage {
	public class MemoryStorageTests : IMailStorageTests<MemoryStorage> {
		
		private MemoryStorage memoryStorage;

		public MemoryStorageTests() {
			this.memoryStorage = new MemoryStorage();
		}
		protected override MemoryStorage MailStorage => memoryStorage;
	}
}
