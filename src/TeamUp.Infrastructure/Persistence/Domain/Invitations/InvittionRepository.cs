using Microsoft.EntityFrameworkCore;

using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Infrastructure.Persistence.Domain.Invitations;

internal sealed class InvitationRepository : IInvitationRepository
{
	private readonly ApplicationDbContext _dbContext;

	public InvitationRepository(ApplicationDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public void AddInvitation(Invitation invitation) => _dbContext.Invitations.Add(invitation);

	public void RemoveInvitation(Invitation invitation) => _dbContext.Invitations.Remove(invitation);

	public async Task<Invitation?> GetInvitationByIdAsync(InvitationId invitationId, CancellationToken ct = default)
	{
		return await _dbContext.Invitations.FindAsync([invitationId], ct);
	}

	public async Task<bool> ExistsInvitationForUserToTeamAsync(UserId userId, TeamId teamId, CancellationToken ct = default)
	{
		return await _dbContext.Invitations.AnyAsync(invitation => invitation.RecipientId == userId && invitation.TeamId == teamId, ct);
	}
}
