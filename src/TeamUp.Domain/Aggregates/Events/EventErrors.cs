using TeamUp.Common;

namespace TeamUp.Domain.Aggregates.Events;

public static class EventErrors
{
	public static readonly ValidationError CannotEndBeforeStart = ValidationError.New("Event cannot end before it starts.", "Events.CannotEndBeforeStart");
	public static readonly ValidationError CannotStartInPast = ValidationError.New("Cannot create event in the past.", "Events.CannotStartInPast");

	public static readonly DomainError NotOpenForResponses = DomainError.New("Event is not open for responses.", "Events.NotOpenForResponses");

	public static readonly NotFoundError EventNotFound = NotFoundError.New("Event not found.", "Events.NotFound");
}
