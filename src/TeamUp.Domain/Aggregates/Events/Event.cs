using TeamUp.Common;
using TeamUp.Common.Abstraction;
using TeamUp.Domain.Aggregates.Events.DomainEvents;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.SeedWork;

namespace TeamUp.Domain.Aggregates.Events;

public sealed record EventId : TypedId<EventId>;

public sealed class Event : AggregateRoot<Event, EventId>
{
	private readonly List<EventResponse> _eventResponses = [];

	public EventTypeId EventTypeId { get; private set; }
	public TeamId TeamId { get; }

	public DateTimeOffset From { get; private set; }
	public DateTimeOffset To { get; private set; }
	public string Description { get; private set; }
	public EventStatus Status { get; private set; }
	public TimeSpan MeetTime { get; private set; }
	public TimeSpan ReplyClosingTimeBeforeMeetTime { get; private set; }
	public IReadOnlyList<EventResponse> EventResponses => _eventResponses.AsReadOnly();

	internal Event(
		EventId id,
		EventTypeId eventTypeId,
		TeamId teamId,
		DateTimeOffset from,
		DateTimeOffset to,
		string description,
		EventStatus status,
		TimeSpan meetTime,
		TimeSpan replyClosingTimeBeforeMeetTime) : base(id)
	{
		EventTypeId = eventTypeId;
		TeamId = teamId;
		From = from;
		To = to;
		Description = description;
		Status = status;
		MeetTime = meetTime;
		ReplyClosingTimeBeforeMeetTime = replyClosingTimeBeforeMeetTime;
	}

	public Result SetMemberResponse(IDateTimeProvider dateTimeProvider, TeamMemberId memberId, EventReply reply)
	{
		return Status
			.Ensure(
				status => status.IsOpenToResponses(),
				DomainError.New($"Event is not open to responses."))
			.Map(_ => GetResponseCloseTime())
			.Ensure(
				responseCloseTime => dateTimeProvider.DateTimeOffsetNow < responseCloseTime,
				DomainError.New("Time for responses ended.")
			)
			.Map(_ => _eventResponses.Find(er => er.TeamMemberId == memberId))
			.Then(response =>
			{
				if (response is null)
					_eventResponses.Add(EventResponse.Create(dateTimeProvider, memberId, Id, reply));
				else
					response.UpdateReply(dateTimeProvider, reply);
			});
	}

	public void UpdateStatus(EventStatus status)
	{
		if (Status == status)
			return;

		Status = status;
		AddDomainEvent(new EventStatusChangedDomainEvent(this));
	}

	public void Update(
		EventTypeId eventTypeId,
		DateTimeOffset from,
		DateTimeOffset to,
		string description,
		TimeSpan meetTime,
		TimeSpan replyCloseBeforeMeetTime)
	{
		var propertyUpdated = false
			| UpdateProperty(x => x.EventTypeId, eventTypeId)
			| UpdateProperty(x => x.From, from)
			| UpdateProperty(x => x.To, to)
			| UpdateProperty(x => x.Description, description)
			| UpdateProperty(x => x.MeetTime, meetTime)
			| UpdateProperty(x => x.ReplyClosingTimeBeforeMeetTime, replyCloseBeforeMeetTime);

		if (propertyUpdated)
			AddDomainEvent(new EventUpdatedDomainEvent(this));
	}

	private DateTimeOffset GetResponseCloseTime() => From - MeetTime - ReplyClosingTimeBeforeMeetTime;

	public static Result<Event> Create(
		EventTypeId eventTypeId,
		TeamId teamId,
		DateTimeOffset from,
		DateTimeOffset to,
		string description,
		TimeSpan meetTime,
		TimeSpan replyClosingTimeBeforeMeetTime,
		IDateTimeProvider dateTimeProvider)
	{
		if (from >= to)
			return ValidationError.New("Event can't end before it starts.");

		if (from >= dateTimeProvider.DateTimeOffsetNow)
			return ValidationError.New("Can't create event in past.");

		return new Event(
			EventId.New(),
			eventTypeId,
			teamId,
			from,
			to,
			description,
			EventStatus.Open,
			meetTime,
			replyClosingTimeBeforeMeetTime
		);
	}
}
