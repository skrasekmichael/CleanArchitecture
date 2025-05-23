﻿using TeamUp.Application.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Application.Teams.SetMemberRole;

internal sealed class SetMemberRoleCommandHandler : ICommandHandler<SetMemberRoleCommand, Result>
{
	private readonly ITeamRepository _teamRepository;
	private readonly IUnitOfWork _unitOfWork;

	public SetMemberRoleCommandHandler(ITeamRepository teamRepository, IUnitOfWork unitOfWork)
	{
		_teamRepository = teamRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> HandleAsync(SetMemberRoleCommand command, CancellationToken ct)
	{
		var team = await _teamRepository.GetTeamByIdAsync(command.TeamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.SetMemberRole(command.InitiatorId, command.MemberId, command.Role))
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
