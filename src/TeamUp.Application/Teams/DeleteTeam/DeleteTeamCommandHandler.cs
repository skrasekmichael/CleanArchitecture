using TeamUp.Application.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.DomainServices;

namespace TeamUp.Application.Teams.DeleteTeam;

internal sealed class DeleteTeamCommandHandler : ICommandHandler<DeleteTeamCommand, Result>
{
	private readonly ITeamDomainService _teamDomainService;
	private readonly IUnitOfWork _unitOfWork;

	public DeleteTeamCommandHandler(ITeamDomainService teamDomainService, IUnitOfWork unitOfWork)
	{
		_teamDomainService = teamDomainService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(DeleteTeamCommand command, CancellationToken ct)
	{
		return await _teamDomainService
			.DeleteTeamAsync(command.InitiatorId, command.TeamId, ct)
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
