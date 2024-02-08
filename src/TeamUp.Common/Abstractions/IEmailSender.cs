namespace TeamUp.Common.Abstractions;

public interface IEmailSender
{
	public Task SendEmailAsync(string email, string subject, string message, CancellationToken ct = default);
}
