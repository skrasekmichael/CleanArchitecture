using TeamUp.Contracts.Invitations;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Domain.DomainServices;

public interface IInvitationDomainService
{
	public Task<Result<InvitationId>> InviteUserAsync(UserId initiatorId, TeamId teamId, string email, CancellationToken ct = default);
	public Task<Result> RemoveInvitationAsync(UserId initiatorId, InvitationId invitationId, CancellationToken ct = default);
}
