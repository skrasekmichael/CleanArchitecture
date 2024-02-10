using System.ComponentModel.DataAnnotations;

using FluentValidation;

using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Contracts.Teams;

public sealed class CreateTeamRequest : IRequestBody
{
	[DataType(DataType.Text)]
	public required string Name { get; init; }

	public sealed class Validator : AbstractValidator<CreateTeamRequest>
	{
		public Validator()
		{
			RuleFor(x => x.Name)
				.NotEmpty()
				.MinimumLength(Team.NAME_MIN_SIZE)
				.MaximumLength(Team.NAME_MAX_SIZE);
		}
	}
}
