using TeamUp.Application.Abstractions;
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

	public async Task<Result> Handle(RemoveTeamMemberCommand command, CancellationToken ct)
	{
		var team = await _teamRepository.GetTeamByIdAsync(command.TeamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.RemoveTeamMember(command.InitiatorId, command.MemberId))
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
