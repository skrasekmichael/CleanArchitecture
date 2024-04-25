using FluentValidation;

using TeamUp.Contracts.Abstractions;

namespace TeamUp.Contracts.Teams;

public sealed record UpdateTeamRoleRequest : IRequestBody
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
