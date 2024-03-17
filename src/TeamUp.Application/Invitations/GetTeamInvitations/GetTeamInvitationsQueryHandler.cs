using Microsoft.EntityFrameworkCore;

using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Invitations;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Application.Invitations.GetTeamInvitations;

internal sealed class GetTeamInvitationsQueryHandler : IQueryHandler<GetTeamInvitationsQuery, Result<List<TeamInvitationResponse>>>
{
	private readonly IAppQueryContext _appQueryContext;

	public GetTeamInvitationsQueryHandler(IAppQueryContext appQueryContext)
	{
		_appQueryContext = appQueryContext;
	}

	public async Task<Result<List<TeamInvitationResponse>>> Handle(GetTeamInvitationsQuery query, CancellationToken ct)
	{
		var teamWithInitiator = await _appQueryContext.Teams
			.Select(team => new
			{
				team.Id,
				Initiaotor = team.Members
					.Select(member => new { member.UserId, member.Role })
					.FirstOrDefault(member => member.UserId == query.InitiatorId),
			})
			.FirstOrDefaultAsync(team => team.Id == query.TeamId, ct);

		return await teamWithInitiator
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.EnsureNotNull(team => team.Initiaotor, TeamErrors.NotMemberOfTeam)
			.Ensure(team => team.Initiaotor!.Role.CanInviteTeamMembers(), TeamErrors.UnauthorizedToReadInvitationList)
			.ThenAsync(team =>
			{
				return _appQueryContext.Invitations
					.Where(invitation => invitation.TeamId == query.TeamId)
					.Select(invitation => new TeamInvitationResponse
					{
						Id = invitation.Id,
						Email = _appQueryContext.Users
							.Select(user => new { user.Id, user.Email })
							.First(user => user.Id == invitation.RecipientId).Email,
						CreatedUtc = invitation.CreatedUtc
					})
					.ToListAsync(ct);
			});
	}
}
