using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Infrastructure.Persistence.Domain.Teams;

internal sealed class TeamRepository : ITeamRepository
{
	public void AddTeam(Team team)
	{
		throw new NotImplementedException();
	}

	public Task<Team?> GetTeamByIdWithTeamMembersAsync(TeamId teamId, CancellationToken ct = default)
	{
		throw new NotImplementedException();
	}
}
