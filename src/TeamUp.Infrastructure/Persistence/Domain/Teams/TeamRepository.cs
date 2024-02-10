﻿using Microsoft.EntityFrameworkCore;

using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Infrastructure.Persistence.Domain.Teams;

internal sealed class TeamRepository : ITeamRepository
{
	private readonly ApplicationDbContext _context;

	public TeamRepository(ApplicationDbContext context)
	{
		_context = context;
	}

	public void AddTeam(Team team) => _context.Teams.Add(team);

	public async Task<Team?> GetTeamByIdAsync(TeamId teamId, CancellationToken ct = default)
	{
		return await _context.Teams
			.Include(team => team.Members)
			.Include(team => team.EventTypes)
			.FirstOrDefaultAsync(team => team.Id == teamId, ct);
	}
}
