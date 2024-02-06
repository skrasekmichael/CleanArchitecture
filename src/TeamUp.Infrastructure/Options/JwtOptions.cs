namespace TeamUp.Infrastructure.Options;

internal sealed class JwtOptions : IApplicationOptions
{
	public static string SectionName => "JwtSettings";

	public required string Issuer { get; init; }
	public required string Audience { get; init; }
	public required string SigningKey { get; init; }
	public required int ExpirationMinutes { get; init; }
}
