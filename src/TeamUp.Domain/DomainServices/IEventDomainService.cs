using TeamUp.Common;
using TeamUp.Contracts.Events;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Domain.DomainServices;

public interface IEventDomainService
{
	public Task<Result<EventId>> CreateEventAsync(
		UserId initiatorId,
		TeamId teamId,
		EventTypeId eventTypeId,
		DateTime fromUtc,
		DateTime toUtc,
		string description,
		TimeSpan meetTime,
		TimeSpan replyClosingTimeBeforeMeetTime,
		CancellationToken ct = default);
}
