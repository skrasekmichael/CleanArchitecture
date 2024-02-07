using System.ComponentModel.DataAnnotations;

using FluentValidation;

namespace TeamUp.Contracts.Users;

public sealed class LoginRequest : IRequestBody
{
	[DataType(DataType.EmailAddress)]
	public required string Email { get; init; }

	[DataType(DataType.Password)]
	public required string Password { get; init; }

	public sealed class Validator : AbstractValidator<LoginRequest>
	{
		public Validator()
		{
			RuleFor(x => x.Email)
				.NotEmpty()
				.EmailAddress();

			RuleFor(x => x.Password)
				.NotEmpty();
		}
	}
}
