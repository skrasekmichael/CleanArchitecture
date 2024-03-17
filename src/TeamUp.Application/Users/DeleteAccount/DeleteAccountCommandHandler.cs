using TeamUp.Application.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Users.DeleteAccount;

internal sealed class DeleteAccountCommandHandler : ICommandHandler<DeleteAccountCommand, Result>
{
	private readonly IUserRepository _userRepository;
	private readonly IPasswordService _passwordService;
	private readonly IUnitOfWork _unitOfWork;

	public DeleteAccountCommandHandler(IUserRepository userRepository, IPasswordService passwordService, IUnitOfWork unitOfWork)
	{
		_userRepository = userRepository;
		_passwordService = passwordService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(DeleteAccountCommand command, CancellationToken ct)
	{
		var user = await _userRepository.GetUserByIdAsync(command.InitiatorId, ct);
		return await user
			.EnsureNotNull(UserErrors.AccountNotFound)
			.Ensure(user => _passwordService.VerifyPassword(command.Password, user.Password), AuthenticationErrors.InvalidCredentials)
			.Tap(user => user.Delete())
			.TapAsync(_ => _unitOfWork.SaveChangesAsync(ct))
			.ToResultAsync();
	}
}
