using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Invitations;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Invitations.RemoveInvitation;

public sealed record RemoveInvitationCommand(UserId InitiatorId, InvitationId InvitationId) : ICommand<Result>;
