using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Events;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Events.RemoveEvent;

public sealed record RemoveEventCommand(UserId InitiatorId, TeamId TeamId, EventId EventId) : ICommand<Result>;
