using Microsoft.EntityFrameworkCore;

using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Teams;

namespace TeamUp.Application.Teams.GetTeam;

internal sealed class GetTeamQueryHandler : IQueryHandler<GetTeamQuery, Result<TeamResponse>>
{
	private readonly IAppQueryContext _appQueryContext;

	public GetTeamQueryHandler(IAppQueryContext appQueryContext)
	{
		_appQueryContext = appQueryContext;
	}

	public async Task<Result<TeamResponse>> Handle(GetTeamQuery request, CancellationToken ct)
	{
		var team = await _appQueryContext.Teams
			.Where(team => team.Id == request.TeamId)
			.Select(team => new TeamResponse
			{
				Name = team.Name,
				Members = team.Members
					.Select(member => new TeamMemberResponse
					{
						Id = member.Id,
						UserId = member.UserId,
						Nickname = member.Nickname,
						Role = member.Role
					})
					.ToList()
					.AsReadOnly()
			})
			.FirstOrDefaultAsync(ct);

		return team
			.EnsureNotNull(NotFoundError.New("Team not found."))
			.Ensure(
				team => team.Members.Any(member => member.UserId == request.InitiatorId),
				AuthorizationError.New("Not member of the team."));
	}
}
