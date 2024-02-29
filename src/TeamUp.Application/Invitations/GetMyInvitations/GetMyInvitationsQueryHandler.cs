using Microsoft.EntityFrameworkCore;

using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Invitations;

namespace TeamUp.Application.Invitations.GetMyInvitations;

internal sealed class GetMyInvitationsQueryHandler : IQueryHandler<GetMyInvitationsQuery, Result<List<InvitationResponse>>>
{
	private readonly IAppQueryContext _appQueryContext;

	public GetMyInvitationsQueryHandler(IAppQueryContext appQueryContext)
	{
		_appQueryContext = appQueryContext;
	}

	public async Task<Result<List<InvitationResponse>>> Handle(GetMyInvitationsQuery query, CancellationToken ct)
	{
		return await _appQueryContext.Invitations
			.Where(invitation => invitation.RecipientId == query.InitiatorId)
			.Select(invitation => new InvitationResponse
			{
				Id = invitation.Id,
				TeamName = _appQueryContext.Teams.First(team => team.Id == invitation.TeamId).Name,
				CreatedUtc = invitation.CreatedUtc
			})
			.ToListAsync(ct);
	}
}
