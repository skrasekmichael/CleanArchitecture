﻿using Microsoft.EntityFrameworkCore;

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
			.Select(team => new
			{
				team.Id,
				Initiaotor = team.Members
					.Select(member => new { member.UserId, member.Role })
					.FirstOrDefault(member => member.UserId == request.InitiatorId),
			})
			.FirstOrDefaultAsync(team => team.Id == request.TeamId, ct);

		return await teamWithInitiator
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.EnsureNotNull(team => team.Initiaotor, TeamErrors.NotMemberOfTeam)
			.Ensure(team => team.Initiaotor!.Role.CanInviteTeamMembers(), TeamErrors.UnauthorizedToReadInvitationList)
			.ThenAsync(team =>
			{
				return _appQueryContext.Invitations
					.Select(invitation => new
					{
						invitation.TeamId,
						invitation.CreatedUtc,
						_appQueryContext.Users
							.Select(user => new { user.Id, user.Email })
							.First(user => user.Id == invitation.RecipientId).Email,
					})
					.Where(invitation => invitation.TeamId == request.TeamId)
					.Select(invitation => new TeamInvitationResponse
					{
						Email = invitation.Email,
						CreatedUtc = invitation.CreatedUtc
					})
					.ToListAsync(ct);
			});
	}
}