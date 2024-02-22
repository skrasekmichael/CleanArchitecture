using TeamUp.Contracts.Invitations;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Domain.Aggregates.Invitations;

public interface IInvitationRepository
{
	public void AddInvitation(Invitation invitation);
	public void RemoveInvitation(Invitation invitation);
	public Task<Invitation?> GetInvitationByIdAsync(InvitationId invitationId, CancellationToken ct = default);
	public Task<bool> ExistsInvitationForUserToTeamAsync(UserId userId, TeamId teamId, CancellationToken ct = default);
}
