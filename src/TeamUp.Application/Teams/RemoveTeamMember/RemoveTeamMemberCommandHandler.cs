using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Application.Teams.RemoveTeamMember;

internal sealed class RemoveTeamMemberCommandHandler : ICommandHandler<RemoveTeamMemberCommand, Result>
{
	private readonly ITeamRepository _teamRepository;
	private readonly IUnitOfWork _unitOfWork;

	public RemoveTeamMemberCommandHandler(ITeamRepository teamRepository, IUnitOfWork unitOfWork)
	{
		_teamRepository = teamRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(RemoveTeamMemberCommand request, CancellationToken ct)
	{
		var team = await _teamRepository.GetTeamByIdAsync(request.TeamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.RemoveTeamMember(request.InitiatorId, request.MemberId))
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
