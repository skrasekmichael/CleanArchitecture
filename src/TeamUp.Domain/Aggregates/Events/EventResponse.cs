using TeamUp.Common.Abstractions;
using TeamUp.Contracts.Events;
using TeamUp.Contracts.Teams;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Events.DomainEvents;

namespace TeamUp.Domain.Aggregates.Events;

public sealed class EventResponse : Entity<EventResponseId>
{
	public TeamMemberId TeamMemberId { get; }

	public EventId EventId { get; }
	public EventReply Reply { get; private set; }
	public DateTime TimeStampUtc { get; private set; }

#pragma warning disable CS8618 // EF Core constructor
	private EventResponse() : base() { }
#pragma warning restore CS8618

	private EventResponse(EventResponseId id, TeamMemberId teamMemberId, EventId eventId, EventReply reply, DateTime timeStampUtc) : base(id)
	{
		TeamMemberId = teamMemberId;
		EventId = eventId;
		Reply = reply;
		TimeStampUtc = timeStampUtc;

		AddDomainEvent(new EventResponseCreatedDomainEvent(this));
	}

	internal void UpdateReply(IDateTimeProvider dateTimeProvider, EventReply reply)
	{
		Reply = reply;
		TimeStampUtc = dateTimeProvider.UtcNow;

		AddDomainEvent(new EventResponseUpdatedDomainEvent(this));
	}

	public static EventResponse Create(IDateTimeProvider dateTimeProvider, TeamMemberId teamMemberId, EventId eventId, EventReply reply) => new(
		EventResponseId.New(),
		teamMemberId,
		eventId,
		reply,
		dateTimeProvider.UtcNow
	);
}
