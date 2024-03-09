using TeamUp.Common.Abstractions;

namespace TeamUp.Tests.EndToEnd.Mocks;

internal sealed class MailInbox : List<(string EmailAddress, string Subject, string Message)>;

internal sealed class EmailSenderMock : IEmailSender
{
	private readonly MailInbox _inbox;

	public EmailSenderMock(MailInbox inbox)
	{
		_inbox = inbox;
	}

	public Task SendEmailAsync(string email, string subject, string message, CancellationToken ct = default)
	{
		_inbox.Add((email, subject, message));
		return Task.CompletedTask;
	}
}
