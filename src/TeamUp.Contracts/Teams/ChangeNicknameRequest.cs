using System.ComponentModel.DataAnnotations;

using FluentValidation;

using TeamUp.Contracts.Abstractions;

namespace TeamUp.Contracts.Teams;

public sealed class ChangeNicknameRequest : IRequestBody
{
	[DataType(DataType.Text)]
	public required string Nickname { get; init; }

	public sealed class Validator : AbstractValidator<ChangeNicknameRequest>
	{
		public Validator()
		{
			RuleFor(x => x.Nickname)
				.NotEmpty()
				.MinimumLength(TeamConstants.NICKNAME_MIN_SIZE)
				.MaximumLength(TeamConstants.NICKNAME_MAX_SIZE);
		}
	}
}
