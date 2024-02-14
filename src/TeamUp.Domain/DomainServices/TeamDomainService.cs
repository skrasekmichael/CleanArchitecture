using TeamUp.Common;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Domain.DomainServices;

internal sealed class TeamDomainService : ITeamDomainService
{
	private readonly ITeamRepository _teamRepository;

	public TeamDomainService(ITeamRepository teamRepository)
	{
		_teamRepository = teamRepository;
	}

	public async Task<Result> DeleteTeamAsync(UserId initiatorId, TeamId teamId, CancellationToken ct = default)
	{
		var team = await _teamRepository.GetTeamByIdAsync(teamId, ct);
		return team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.And(team => team.GetTeamMemberByUserId(initiatorId))
			.Ensure((_, teamMember) => TeamRules.MemberIsOwner(teamMember), TeamErrors.UnauthorizedToDeleteTeam)
			.Tap((team, _) => _teamRepository.RemoveTeam(team))
			.ToResult();
	}
}
