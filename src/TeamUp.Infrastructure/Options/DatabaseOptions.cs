using TeamUp.Common.Abstraction;

namespace TeamUp.Infrastructure.Options;

internal sealed class DatabaseOptions : IApplicationOptions
{
	public static string SectionName => "Database";

	public required string ConnectionString { get; init; }
}
