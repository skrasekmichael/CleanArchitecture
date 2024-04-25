using System.ComponentModel.DataAnnotations;

using FluentValidation;

using TeamUp.Contracts.Abstractions;

namespace TeamUp.Contracts.Teams;

public sealed record CreateTeamRequest : IRequestBody
{
	[DataType(DataType.Text)]
	public required string Name { get; init; }

	public sealed class Validator : AbstractValidator<CreateTeamRequest>
	{
		public Validator()
		{
			RuleFor(x => x.Name)
				.NotEmpty()
				.MinimumLength(TeamConstants.TEAM_NAME_MIN_SIZE)
				.MaximumLength(TeamConstants.TEAM_NAME_MAX_SIZE);
		}
	}
}
