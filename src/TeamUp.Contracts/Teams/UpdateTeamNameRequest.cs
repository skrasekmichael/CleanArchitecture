using System.ComponentModel.DataAnnotations;

using FluentValidation;

using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Contracts.Teams;

public sealed class UpdateTeamNameRequest : IRequestBody
{
	[DataType(DataType.Text)]
	public required string Name { get; init; }

	public sealed class Validator : AbstractValidator<UpdateTeamNameRequest>
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
