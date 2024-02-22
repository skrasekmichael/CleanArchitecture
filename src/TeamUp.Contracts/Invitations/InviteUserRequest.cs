using System.ComponentModel.DataAnnotations;

using FluentValidation;

using TeamUp.Contracts.Abstractions;
using TeamUp.Contracts.Teams;

namespace TeamUp.Contracts.Invitations;

public sealed class InviteUserRequest : IRequestBody
{
	public required TeamId TeamId { get; init; }

	[DataType(DataType.EmailAddress)]
	public required string Email { get; init; }

	public sealed class Validator : AbstractValidator<InviteUserRequest>
	{
		public Validator()
		{
			RuleFor(x => x.TeamId).NotEmpty();

			RuleFor(x => x.Email)
				.NotEmpty()
				.EmailAddress();
		}
	}
}
