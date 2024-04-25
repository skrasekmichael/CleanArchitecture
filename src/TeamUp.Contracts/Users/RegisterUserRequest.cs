using System.ComponentModel.DataAnnotations;

using FluentValidation;

using TeamUp.Contracts.Abstractions;

namespace TeamUp.Contracts.Users;

public sealed record RegisterUserRequest : IRequestBody
{
	[DataType(DataType.Text)]
	public required string Name { get; init; }

	[DataType(DataType.EmailAddress)]
	public required string Email { get; init; }

	[DataType(DataType.Password)]
	public required string Password { get; init; }

	public sealed class Validator : AbstractValidator<RegisterUserRequest>
	{
		public Validator()
		{
			RuleFor(x => x.Name)
				.NotEmpty()
				.MinimumLength(UserConstants.USERNAME_MIN_SIZE)
				.MaximumLength(UserConstants.USERNAME_MAX_SIZE);

			RuleFor(x => x.Email)
				.NotEmpty()
				.EmailAddress();

			//TODO: configure password requirements
			RuleFor(x => x.Password)
				.NotEmpty();
		}
	}
}
