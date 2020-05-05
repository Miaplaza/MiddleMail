using MiaPlaza.MiddleMail.Storage;

namespace MiaPlaza.MiddleMail.Tests.Storage {
	public class MemoryStorageTests : IMailStorageTests<MemoryStorage> {
		
		private MemoryStorage memoryStorage;

		public MemoryStorageTests() {
			this.memoryStorage = new MemoryStorage();
		}
		protected override MemoryStorage MailStorage => memoryStorage;
	}
}
