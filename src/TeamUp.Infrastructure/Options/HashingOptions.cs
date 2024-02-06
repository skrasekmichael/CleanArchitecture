namespace TeamUp.Infrastructure.Options;

internal sealed class HashingOptions : IApplicationOptions
{
	public static string SectionName => "Hashing";

	public required int Pbkdf2Iterations { get; set; }

	public required string Pepper { get; set; }
}
