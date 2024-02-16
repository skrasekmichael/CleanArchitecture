using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Application.Teams.SetMemberNickname;

internal sealed class ChangeNicknameCommandHandler : ICommandHandler<ChangeNicknameCommand, Result>
{
	private readonly ITeamRepository _teamRepository;
	private readonly IUnitOfWork _unitOfWork;

	public ChangeNicknameCommandHandler(ITeamRepository teamRepository, IUnitOfWork unitOfWork)
	{
		_teamRepository = teamRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(ChangeNicknameCommand request, CancellationToken ct)
	{
		var team = await _teamRepository.GetTeamByIdAsync(request.TeamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.ChangeNickname(request.InitiatorId, request.Nickname))
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
