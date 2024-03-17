using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Domain.DomainServices;

public interface ITeamDomainService
{
	public Task<Result> DeleteTeamAsync(UserId initiatorId, TeamId teamId, CancellationToken ct = default);
}
