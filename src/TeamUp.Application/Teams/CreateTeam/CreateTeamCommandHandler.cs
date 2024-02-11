using TeamUp.Api.Endpoints.Teams;
using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Common.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Teams.CreateTeam;

internal sealed class CreateTeamCommandHandler : ICommandHandler<CreateTeamCommand, Result<TeamId>>
{
	private readonly ITeamRepository _teamRepository;
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly IUserRepository _userRepository;
	private readonly IUnitOfWork _unitOfWork;

	public CreateTeamCommandHandler(ITeamRepository teamRepository, IDateTimeProvider dateTimeProvider, IUserRepository userRepository, IUnitOfWork unitOfWork)
	{
		_teamRepository = teamRepository;
		_dateTimeProvider = dateTimeProvider;
		_userRepository = userRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<TeamId>> Handle(CreateTeamCommand request, CancellationToken ct)
	{
		var user = await _userRepository.GetUserByIdAsync(request.OwnerId, ct);
		return await user
			.EnsureNotNull(UserErrors.AccountNotFound)
			.Then(user => Team.Create(request.Name, user, _dateTimeProvider))
			.Tap(_teamRepository.AddTeam)
			.Then(team => team.Id)
			.TapAsync(_ => _unitOfWork.SaveChangesAsync(ct));
	}
}
