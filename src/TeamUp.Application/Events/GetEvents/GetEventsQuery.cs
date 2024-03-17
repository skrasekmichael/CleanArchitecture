using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Events;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Events.GetEvents;

public sealed record GetEventsQuery(UserId InitiatorId, TeamId TeamId, DateTime? FromUtc = null) : IQuery<Result<List<EventSlimResponse>>>;
