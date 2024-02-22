using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Teams.CreateEventType;

public sealed record CreateEventTypeCommand(UserId InitiatorId, TeamId TeamId, string Name, string Description) : ICommand<Result<EventTypeId>>;
