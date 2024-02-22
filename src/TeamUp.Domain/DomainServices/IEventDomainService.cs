using TeamUp.Common;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;
using TeamUp.Domain.Aggregates.Events;

namespace TeamUp.Domain.DomainServices;

public interface IEventDomainService
{
	public Task<Result<Event>> CreateEventAsync(UserId loggedUserId, TeamId teamId, EventTypeId eventTypeId, DateTimeOffset from, DateTimeOffset to, string description, TimeSpan meetTime, TimeSpan replyClosingTimeBeforeMeetTime, CancellationToken ct = default);
}
