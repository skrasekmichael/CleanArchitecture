namespace TeamUp.Domain.Aggregates.Teams;

public interface ITeamRepository
{
	Task<Team?> GetTeamByIdWithTeamMembersAsync(TeamId teamId, CancellationToken ct = default);
	Task AddTeamAsync(Team team, CancellationToken ct = default);
}
