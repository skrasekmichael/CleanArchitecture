using TeamUp.Application.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Users.Activation;

internal sealed class ActivateAccountCommandHandler : ICommandHandler<ActivateAccountCommand, Result>
{
	private readonly IUserRepository _userRepository;
	private readonly IUnitOfWork _unitOfWork;

	public ActivateAccountCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
	{
		_userRepository = userRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(ActivateAccountCommand command, CancellationToken ct)
	{
		var user = await _userRepository.GetUserByIdAsync(command.UserId, ct);
		return await user
			.EnsureNotNull(UserErrors.UserNotFound)
			.Then(user => user.Activate())
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
