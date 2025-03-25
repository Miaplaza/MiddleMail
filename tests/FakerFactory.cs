using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using MiddleMail;
using MiddleMail.Model;
using MimeKit;

namespace MiddleMail.Tests {

	public static class FakerFactory {

		public static Faker<EmailMessage> EmailMessageFaker = new Faker<EmailMessage>()
			.CustomInstantiator(f => new EmailMessage(
				id: Guid.NewGuid(),
				from: (f.Name.FullName(), f.Internet.Email()),
				to: (f.Name.FullName(), f.Internet.Email()),
				cc: generateCcList(f),
				replyTo: (f.Name.FullName(), f.Internet.Email()),
				subject: f.Lorem.Sentence(),
				plainText: f.Lorem.Sentences(),
				htmlText: f.Lorem.Sentences(),
				headers: null,
				tags: f.Lorem.Words().ToList()
			));

		/// <summary>
		/// Generates a list of CC recipients with a fixed size of 3.
		/// </summary>
		/// <remarks>
		/// The size is arbitrary since parsing behavior is consistent across non-empty lists.
		/// </remarks>
		private static List<(string name, string address)> generateCcList(Faker f) {
			const int count = 3;
			return Enumerable.Range(0, count)
				.Select(_ => (f.Name.FullName(), f.Internet.Email()))
				.ToList();
		}

		public static Faker<MimeMessage> MimeMessageFaker = new Faker<MimeMessage>()
			.CustomInstantiator(f => new MimeMessage(
				from: new MailboxAddress[] { new MailboxAddress(f.Name.FullName(), f.Internet.Email()) },
				to: new MailboxAddress[] { new MailboxAddress(f.Name.FullName(), f.Internet.Email()) },
				subject: f.Lorem.Sentence(),
				body: new TextPart("plain") { Text = f.Lorem.Sentences(10) }
			));
	}
}
