using TeamUp.Common;
using TeamUp.Domain.Aggregates.Events;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Domain.DomainServices;
public interface IEventDomainService
{
	public Task<Result<Event>> CreateEventAsync(UserId loggedUserId, TeamId teamId, EventTypeId eventTypeId, DateTimeOffset from, DateTimeOffset to, string description, TimeSpan meetTime, TimeSpan replyClosingTimeBeforeMeetTime, CancellationToken ct = default);
}
