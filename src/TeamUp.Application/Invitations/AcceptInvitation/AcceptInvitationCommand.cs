using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Invitations;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Invitations.AcceptInvitation;

public sealed record AcceptInvitationCommand(UserId InitiatorId, InvitationId InvitationId) : ICommand<Result>;
