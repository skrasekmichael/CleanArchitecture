using FluentValidation;

using TeamUp.Contracts.Abstractions;

namespace TeamUp.Contracts.Events;

public sealed class UpsertEventReplyRequest : IRequestBody
{
	public required ReplyType ReplyType { get; init; }
	public required string Message { get; init; }

	public sealed class Validator : AbstractValidator<UpsertEventReplyRequest>
	{
		public Validator()
		{
			RuleFor(x => x.ReplyType).IsInEnum();

			RuleFor(x => x.Message)
				.Empty().When(x => x.ReplyType == ReplyType.Yes, ApplyConditionTo.CurrentValidator)
				.NotEmpty().When(x => x.ReplyType == ReplyType.No || x.ReplyType == ReplyType.Maybe, ApplyConditionTo.CurrentValidator)
				.MaximumLength(80);
		}
	}
}
