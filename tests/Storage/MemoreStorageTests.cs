using MiddleMail.Storage.Memory;

namespace MiddleMail.Tests.Storage {
	public class MemoryStorageTests : IMailStorageTests {
		
		private MemoryStorage memoryStorage;

		public MemoryStorageTests() {
			this.memoryStorage = new MemoryStorage();
		}
		protected override IMailStorage MailStorage => memoryStorage;
	}
}
