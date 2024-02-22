﻿using TeamUp.Contracts.Teams;

namespace TeamUp.Domain.Aggregates.Teams;

public interface ITeamRepository
{
	public Task<Team?> GetTeamByIdAsync(TeamId teamId, CancellationToken ct = default);
	public void AddTeam(Team team);
	public void RemoveTeam(Team team);
}
