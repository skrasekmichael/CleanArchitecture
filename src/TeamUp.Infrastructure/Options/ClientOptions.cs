using System.ComponentModel.DataAnnotations;

namespace TeamUp.Infrastructure.Options;

internal sealed class ClientOptions : IApplicationOptions
{
	public static string SectionName => "Client";

	[Required]
	public required string Url { get; init; }

	[Required]
	public required string ActivateAccountUrl { get; init; }

	[Required]
	public required string CompleteAccountRegistrationUrl { get; init; }
}
