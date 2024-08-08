namespace MiddleMail.Model;

public enum MailUrgency { Transactional, Bulk }

public static class MailUrgencyExtensions {
	public static string Topic(this MailUrgency urgency) => urgency switch {
		MailUrgency.Transactional => "Transactional",
		MailUrgency.Bulk => "Bulk",
		_ => throw new System.NotImplementedException()
	};
}
