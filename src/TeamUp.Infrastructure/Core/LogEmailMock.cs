using Microsoft.Extensions.Logging;

using TeamUp.Common.Abstractions;

namespace TeamUp.Infrastructure.Core;

internal sealed class LogEmailMock : IEmailSender
{
	private readonly ILogger<EmailSender> _logger;

	public LogEmailMock(ILogger<EmailSender> logger)
	{
		_logger = logger;
	}

	public Task SendEmailAsync(string email, string subject, string message, CancellationToken ct = default)
	{
		_logger.LogInformation("Email to: {email}\n\tSubject: {subject}\n\tBody:\n{message}", email, subject, message);
		return Task.CompletedTask;
	}
}
