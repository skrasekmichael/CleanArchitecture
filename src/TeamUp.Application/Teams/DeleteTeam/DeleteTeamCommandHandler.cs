using TeamUp.Application.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Application.Teams.DeleteTeam;

internal sealed class DeleteTeamCommandHandler : ICommandHandler<DeleteTeamCommand, Result>
{
	private readonly ITeamRepository _teamRepository;
	private readonly IUnitOfWork _unitOfWork;

	public DeleteTeamCommandHandler(ITeamRepository teamDomainService, IUnitOfWork unitOfWork)
	{
		_teamRepository = teamDomainService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(DeleteTeamCommand command, CancellationToken ct)
	{
		var team = await _teamRepository.GetTeamByIdAsync(command.TeamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.Delete(command.InitiatorId))
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
