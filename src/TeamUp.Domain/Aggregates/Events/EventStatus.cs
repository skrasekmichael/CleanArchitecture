namespace TeamUp.Domain.Aggregates.Events;

public enum EventStatus
{
	Open = 0,
	Closed = 1,
	Canceled = 2,
}

public static class EventStatusExtensions
{
	public static bool IsOpenToResponses(this EventStatus status) => status == EventStatus.Open;
}
