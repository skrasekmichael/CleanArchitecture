using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Application.Teams.SetTeamName;

internal sealed class SetTeamNameCommandHandler : ICommandHandler<SetTeamNameCommand, Result>
{
	private readonly ITeamRepository _teamRepository;
	private readonly IUnitOfWork _unitOfWork;

	public SetTeamNameCommandHandler(ITeamRepository teamRepository, IUnitOfWork unitOfWork)
	{
		_teamRepository = teamRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(SetTeamNameCommand request, CancellationToken ct)
	{
		var team = await _teamRepository.GetTeamByIdAsync(request.TeamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.ChangeTeamName(request.InitiatorId, request.Name))
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
