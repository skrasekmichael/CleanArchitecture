using System.ComponentModel.DataAnnotations;

using FluentValidation;

using TeamUp.Contracts.Abstractions;

namespace TeamUp.Contracts.Teams;

public sealed class UpsertEventTypeRequest : IRequestBody
{
	[DataType(DataType.Text)]
	public required string Name { get; init; }

	[DataType(DataType.Text)]
	public required string Description { get; init; }

	public sealed class Validator : AbstractValidator<UpsertEventTypeRequest>
	{
		public Validator()
		{
			RuleFor(x => x.Name).NotEmpty();
			RuleFor(x => x.Description).NotEmpty();
		}
	}
}
