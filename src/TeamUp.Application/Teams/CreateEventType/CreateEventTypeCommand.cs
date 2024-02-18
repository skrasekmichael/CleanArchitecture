using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Teams.CreateEventType;

public sealed record CreateEventTypeCommand(UserId InitiatorId, TeamId TeamId, string Name, string Description) : ICommand<Result<EventTypeId>>;
