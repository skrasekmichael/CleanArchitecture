using System.ComponentModel.DataAnnotations;

using FluentValidation;

using TeamUp.Contracts.Abstractions;

namespace TeamUp.Contracts.Users;

public sealed record LoginRequest : IRequestBody
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
