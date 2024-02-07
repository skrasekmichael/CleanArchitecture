using System.ComponentModel.DataAnnotations;

using FluentValidation;

using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Contracts.Users;

public sealed class RegisterUserRequest : IRequestBody
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
				.MinimumLength(User.NAME_MIN_SIZE)
				.MaximumLength(User.NAME_MAX_SIZE);

			RuleFor(x => x.Email)
				.NotEmpty()
				.EmailAddress();

			//TODO: configure password requirements
			RuleFor(x => x.Password)
				.NotEmpty();
		}
	}
}
