using TeamUp.Contracts.Events;

namespace TeamUp.Domain.Aggregates.Events;

public static class EventErrors
{
	public static readonly ValidationError EventDescriptionMaxSize = new("Events.Validation.DescriptionMaxSize", $"Event's description must be shorter than {EventConstants.EVENT_DESCRIPTION_MAX_SIZE} characters.");

	public static readonly ValidationError CannotEndBeforeStart = new("Events.Validation.StartBeforeEnd", "Event cannot end before it starts.");
	public static readonly ValidationError CannotStartInPast = new("Events.Validation.StartInPast", "Cannot create event in the past.");

	public static readonly DomainError NotOpenForResponses = new("Events.Domain.ClosedForResponses", "Event is not open for responses.");
	public static readonly DomainError TimeForResponsesExpired = new("Events.Domain.RespondTimeExpired", "Time for responses expired.");

	public static readonly NotFoundError EventNotFound = new("Events.NotFound", "Event not found.");
}
