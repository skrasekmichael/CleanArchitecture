using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Teams;

namespace TeamUp.Application.Teams.GetUserTeams;

internal sealed class GetUserTeamsQueryHandler : IQueryHandler<GetUserTeamsQuery, Result<List<TeamSlimResponse>>>
{
	private readonly IAppQueryContext _appQueryContext;

	public GetUserTeamsQueryHandler(IAppQueryContext appQueryContext)
	{
		_appQueryContext = appQueryContext;
	}

	public async Task<Result<List<TeamSlimResponse>>> HandleAsync(GetUserTeamsQuery query, CancellationToken ct)
	{
		return await _appQueryContext.Teams
			.Where(team => team.Members.Any(member => member.UserId == query.InitiatorId))
			.Select(team => new TeamSlimResponse
			{
				TeamId = team.Id,
				Name = team.Name,
				NumberOfTeamMembers = team.NumberOfMembers
			})
			.ToListAsync(ct);
	}
}
