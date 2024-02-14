using TeamUp.Common;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Domain.DomainServices;

public interface ITeamDomainService
{
	public Task<Result> DeleteTeamAsync(UserId initiatorId, TeamId teamId, CancellationToken ct = default);
}
