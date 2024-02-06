namespace TeamUp.Infrastructure.Options;

internal sealed class ClientOptions : IApplicationOptions
{
	public static string SectionName => "Client";

	public required string Url { get; init; }
}
