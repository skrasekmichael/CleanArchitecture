using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using TeamUp.Common.Abstractions;
using TeamUp.Infrastructure.Options;

namespace TeamUp.Infrastructure.Core;

internal sealed class EmailSender : IEmailSender
{
	private readonly IOptions<EmailOptions> _emailOptions;
	private readonly ILogger<EmailSender> _logger;

	public EmailSender(IOptions<EmailOptions> emailOptions, ILogger<EmailSender> logger)
	{
		_emailOptions = emailOptions;
		_logger = logger;
	}

	public async Task SendEmailAsync(string email, string subject, string message, CancellationToken ct = default)
	{
		var body = new MimeMessage();
		body.From.Add(new MailboxAddress(_emailOptions.Value.SenderName, _emailOptions.Value.SenderEmail));
		body.To.Add(new MailboxAddress(string.Empty, email));
		body.Subject = subject;
		body.Body = new TextPart("plain")
		{
			Text = message
		};

		try
		{
			using var client = new SmtpClient();
			client.Connect(_emailOptions.Value.Server, _emailOptions.Value.Port, false, ct);
			client.Authenticate(_emailOptions.Value.UserName, _emailOptions.Value.Password, ct);

			var response = await client.SendAsync(body, ct);
			_logger.LogInformation("Mail Server response {response}", response);
			client.Disconnect(true, ct);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send email to {emailAddress}.", email);
		}
	}
}
