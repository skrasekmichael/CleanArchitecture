using TeamUp.Application.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Application.Teams.ChangeOwnership;

internal sealed class ChangeOwnershipCommandHandler : ICommandHandler<ChangeOwnershipCommand, Result>
{
	private readonly ITeamRepository _teamRepository;
	private readonly IUnitOfWork _unitOfWork;

	public ChangeOwnershipCommandHandler(ITeamRepository teamRepository, IUnitOfWork unitOfWork)
	{
		_teamRepository = teamRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(ChangeOwnershipCommand command, CancellationToken ct)
	{
		var team = await _teamRepository.GetTeamByIdAsync(command.TeamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.ChangeOwnership(command.InitiatorId, command.NewOwnerId))
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
