using System;
using Bogus;
using MiaPlaza.MiddleMail;
using MiaPlaza.MiddleMail.Model;
using MimeKit;

namespace MiaPlaza.MiddleMail.Tests {

	public static class FakerFactory {

		public static Faker<EmailMessage> EmailMessageFaker = new Faker<EmailMessage>()
			.CustomInstantiator(f => new EmailMessage(
				id: Guid.NewGuid(),
				fromEmail: f.Internet.Email(),
				fromName: f.Name.FullName(),
				toEmail: f.Internet.Email(),
				toName: f.Name.FullName(),
				subject: f.Lorem.Sentence(),
				plainText: f.Lorem.Sentences(),
				htmlText: f.Lorem.Sentences()));

		public static Faker<MimeMessage> MimeMessageFaker = new Faker<MimeMessage>()
			.CustomInstantiator(f => new MimeMessage(
				from: new MailboxAddress[] { new MailboxAddress(f.Name.FullName(), f.Internet.Email())},
				to: new MailboxAddress[] { new MailboxAddress(f.Name.FullName(), f.Internet.Email())},
				subject: f.Lorem.Sentence(),
				body: new TextPart("plain") { Text = f.Lorem.Sentences(10)}));
	}
}
