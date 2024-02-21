using Microsoft.EntityFrameworkCore;

using TeamUp.Application.Abstractions;
using TeamUp.Common;
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

	public async Task<Result<List<TeamInvitationResponse>>> Handle(GetTeamInvitationsQuery request, CancellationToken ct)
	{
		var teamWithInitiator = await _appQueryContext.Teams
			.Where(team => team.Id == request.TeamId)
			.Select(team => new
			{
				Initiaotor = team.Members.FirstOrDefault(member => member.UserId == request.InitiatorId),
			})
			.FirstOrDefaultAsync(ct);

		return await teamWithInitiator
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.EnsureNotNull(team => team.Initiaotor, TeamErrors.NotMemberOfTeam)
			.Ensure(team => team.Initiaotor!.Role.CanInviteTeamMembers(), TeamErrors.UnauthorizedToReadInvitationList)
			.ThenAsync(team =>
			{
				return _appQueryContext.Invitations
					.Where(invitation => invitation.TeamId == request.TeamId)
					.Select(invitation => new TeamInvitationResponse
					{
						Email = _appQueryContext.Users.First(user => user.Id == invitation.RecipientId).Email,
						CreatedUtc = invitation.CreatedUtc
					})
					.ToListAsync(ct);
			});
	}
}
