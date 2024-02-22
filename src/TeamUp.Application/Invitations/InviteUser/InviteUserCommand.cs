using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Invitations;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Invitations.InviteUser;

public sealed record InviteUserCommand(UserId InitiatorId, TeamId TeamId, string Email) : ICommand<Result<InvitationId>>;
