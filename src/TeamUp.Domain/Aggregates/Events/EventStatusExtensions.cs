using TeamUp.Contracts.Events;

namespace TeamUp.Domain.Aggregates.Events;

public static class EventStatusExtensions
{
	public static bool IsOpenForResponses(this EventStatus status) => status == EventStatus.Open;
}
