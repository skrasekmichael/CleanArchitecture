using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Events;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Events.GetEvent;

public sealed record GetEventQuery(UserId InitiatorId, TeamId TeamId, EventId EventId) : IQuery<Result<EventResponse>>;
