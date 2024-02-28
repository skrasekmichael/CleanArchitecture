using TeamUp.Common;
using TeamUp.Common.Abstractions;
using TeamUp.Contracts.Events;
using TeamUp.Contracts.Teams;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Events.DomainEvents;

namespace TeamUp.Domain.Aggregates.Events;

public sealed class Event : AggregateRoot<Event, EventId>
{
	private readonly List<EventResponse> _eventResponses = [];

	public EventTypeId EventTypeId { get; private set; }
	public TeamId TeamId { get; }

	public DateTime FromUtc { get; private set; }
	public DateTime ToUtc { get; private set; }
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
		DateTime fromUtc,
		DateTime toUtc,
		string description,
		EventStatus status,
		TimeSpan meetTime,
		TimeSpan replyClosingTimeBeforeMeetTime) : base(id)
	{
		EventTypeId = eventTypeId;
		TeamId = teamId;
		FromUtc = fromUtc;
		ToUtc = toUtc;
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
			.Ensure(responseCloseTime => IsNotClosed(dateTimeProvider, responseCloseTime), EventErrors.NotOpenForResponses)
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
			| UpdateProperty(x => x.FromUtc, from)
			| UpdateProperty(x => x.ToUtc, to)
			| UpdateProperty(x => x.Description, description)
			| UpdateProperty(x => x.MeetTime, meetTime)
			| UpdateProperty(x => x.ReplyClosingTimeBeforeMeetTime, replyCloseBeforeMeetTime);

		if (propertyUpdated)
			AddDomainEvent(new EventUpdatedDomainEvent(this));
	}

	private DateTime GetResponseCloseTime() => FromUtc - MeetTime - ReplyClosingTimeBeforeMeetTime;

	public static Result<Event> Create(
		EventTypeId eventTypeId,
		TeamId teamId,
		DateTime fromUtc,
		DateTime toUtc,
		string description,
		TimeSpan meetTime,
		TimeSpan replyClosingTimeBeforeMeetTime,
		IDateTimeProvider dateTimeProvider)
	{
		return fromUtc
			.Ensure(from => from < toUtc, EventErrors.CannotEndBeforeStart)
			.Ensure(from => from > dateTimeProvider.DateTimeOffsetUtcNow, EventErrors.CannotStartInPast)
			.Then(_ => new Event(
				EventId.New(),
				eventTypeId,
				teamId,
				fromUtc,
				toUtc,
				description,
				EventStatus.Open,
				meetTime,
				replyClosingTimeBeforeMeetTime
			));
	}
}
