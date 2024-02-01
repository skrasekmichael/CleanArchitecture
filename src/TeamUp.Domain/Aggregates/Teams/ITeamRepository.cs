namespace TeamUp.Domain.Aggregates.Teams;

public interface ITeamRepository
{
	public Task<Team?> GetTeamByIdWithTeamMembersAsync(TeamId teamId, CancellationToken ct = default);
	public void AddTeam(Team team);
}
