using FluentValidation;

using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Contracts.Teams;

public sealed class UpdateTeamRoleRequest : IRequestBody
{
	public required TeamRole Role { get; init; }

	public sealed class Validator : AbstractValidator<UpdateTeamRoleRequest>
	{
		public Validator()
		{
			RuleFor(x => x.Role).IsInEnum();
		}
	}
}
