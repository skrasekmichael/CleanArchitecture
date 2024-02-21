using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Invitations.InviteUser;

public sealed record InviteUserCommand(UserId InitiatorId, TeamId TeamId, string Email) : ICommand<Result<InvitationId>>;
