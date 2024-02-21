using TeamUp.Common;
using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Domain.DomainServices;

public interface IInvitationDomainService
{
	public Task<Result<InvitationId>> InviteUserAsync(UserId initiatorId, TeamId teamId, string email, CancellationToken ct = default);
}
