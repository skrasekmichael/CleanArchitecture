using TeamUp.Application.Abstractions;
using TeamUp.Common;
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

	public async Task<Result> Handle(DeleteTeamCommand request, CancellationToken ct)
	{
		return await _teamDomainService
			.DeleteTeamAsync(request.InitiatorId, request.TeamId, ct)
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
