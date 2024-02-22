﻿using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Application.Teams.CreateEventType;

internal sealed class CreateEventTypeCommandHandler : ICommandHandler<CreateEventTypeCommand, Result<EventTypeId>>
{
	private readonly ITeamRepository _teamRepository;
	private readonly IUnitOfWork _unitOfWork;

	public CreateEventTypeCommandHandler(ITeamRepository teamRepository, IUnitOfWork unitOfWork)
	{
		_teamRepository = teamRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<EventTypeId>> Handle(CreateEventTypeCommand request, CancellationToken ct)
	{
		var team = await _teamRepository.GetTeamByIdAsync(request.TeamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.CreateEventType(request.InitiatorId, request.Name, request.Description))
			.TapAsync(_ => _unitOfWork.SaveChangesAsync(ct));
	}
}