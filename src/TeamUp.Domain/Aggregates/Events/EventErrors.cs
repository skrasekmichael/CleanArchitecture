using TeamUp.Common;
using TeamUp.Contracts.Events;

namespace TeamUp.Domain.Aggregates.Events;

public static class EventErrors
{
	public static readonly ValidationError EventDescriptionMaxSize = ValidationError.New($"Event's description must be shorter than {EventConstants.EVENT_DESCRIPTION_MAX_SIZE} characters.", "Events.DescriptionMaxSize");

	public static readonly ValidationError CannotEndBeforeStart = ValidationError.New("Event cannot end before it starts.", "Events.CannotEndBeforeStart");
	public static readonly ValidationError CannotStartInPast = ValidationError.New("Cannot create event in the past.", "Events.CannotStartInPast");

	public static readonly DomainError NotOpenForResponses = DomainError.New("Event is not open for responses.", "Events.NotOpenForResponses");
	public static readonly DomainError TimeForResponsesExpired = DomainError.New("Time for responses expired.", "Events.TimeForResponsesExpired");

	public static readonly NotFoundError EventNotFound = NotFoundError.New("Event not found.", "Events.NotFound");

}
