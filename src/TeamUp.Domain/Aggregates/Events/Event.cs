using TeamUp.Common;
using TeamUp.Common.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Events.DomainEvents;
using TeamUp.Domain.Aggregates.Teams;

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

#pragma warning disable CS8618 // EF Core constructor
	private Event() : base() { }
#pragma warning restore CS8618

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
		static bool IsNotClosed(IDateTimeProvider dateTimeProvider, DateTimeOffset responseCloseTime) => dateTimeProvider.DateTimeOffsetUtcNow < responseCloseTime;

		return Status
			.Ensure(status => status.IsOpenForResponses(), EventErrors.NotOpenForResponses)
			.Then(_ => GetResponseCloseTime())
			.Ensure(responseCloseTime => IsNotClosed(dateTimeProvider, responseCloseTime), EventErrors.ClosedForResponses)
			.Then(_ => _eventResponses.Find(er => er.TeamMemberId == memberId))
			.Tap(response =>
			{
				if (response is null)
					_eventResponses.Add(EventResponse.Create(dateTimeProvider, memberId, Id, reply));
				else
					response.UpdateReply(dateTimeProvider, reply);
			})
			.ToResult();
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
		return from
			.Ensure(from => from < to, EventErrors.CannotEndBeforeStart)
			.Ensure(from => from > dateTimeProvider.DateTimeOffsetUtcNow, EventErrors.CannotStartInPast)
			.Then(_ => new Event(
				EventId.New(),
				eventTypeId,
				teamId,
				from,
				to,
				description,
				EventStatus.Open,
				meetTime,
				replyClosingTimeBeforeMeetTime
			));
	}
}
