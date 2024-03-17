using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Invitations;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Invitations.GetMyInvitations;

public sealed record GetMyInvitationsQuery(UserId InitiatorId) : IQuery<Result<List<InvitationResponse>>>;
