namespace TeamUp.Infrastructure.Options;

internal sealed class EmailOptions : IApplicationOptions
{
	public static string SectionName => "MailSettings";

	public required string Server { get; init; }
	public required int Port { get; init; }
	public required string SenderName { get; init; }
	public required string SenderEmail { get; init; }
	public required string UserName { get; init; }
	public required string Password { get; init; }
}
