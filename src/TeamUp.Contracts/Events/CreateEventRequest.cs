using System.ComponentModel.DataAnnotations;

using FluentValidation;

using TeamUp.Common.Abstractions;
using TeamUp.Contracts.Abstractions;
using TeamUp.Contracts.Teams;

namespace TeamUp.Contracts.Events;

public sealed class CreateEventRequest : IRequestBody
{
	public required EventTypeId EventTypeId { get; init; }

	[DataType(DataType.DateTime)]
	public required DateTime FromUtc { get; init; }

	[DataType(DataType.DateTime)]
	public required DateTime ToUtc { get; init; }

	[DataType(DataType.Text)]
	public required string Description { get; init; }

	[DataType(DataType.Time)]
	public required TimeSpan MeetTime { get; init; }

	[DataType(DataType.Time)]
	public required TimeSpan ReplyClosingTimeBeforeMeetTime { get; init; }

	public sealed class Validator : AbstractValidator<CreateEventRequest>
	{
		public Validator(IDateTimeProvider dateTimeProvider)
		{
			RuleFor(x => x.EventTypeId).NotEmpty();

			RuleFor(x => x.FromUtc)
				.NotEmpty()
				.GreaterThan(dateTimeProvider.UtcNow)
				.WithMessage("Cannot create event in past.");

			RuleFor(x => x.ToUtc)
				.NotEmpty()
				.Must((model, to) => model.FromUtc < to)
				.WithMessage("Event cannot end before it starts.");

			RuleFor(x => x.Description)
				.NotEmpty()
				.MaximumLength(EventConstants.EVENT_DESCRIPTION_MAX_SIZE);

			RuleFor(x => x.MeetTime).GreaterThan(TimeSpan.Zero);

			RuleFor(x => x.ReplyClosingTimeBeforeMeetTime).GreaterThan(TimeSpan.Zero);
		}
	}
}
