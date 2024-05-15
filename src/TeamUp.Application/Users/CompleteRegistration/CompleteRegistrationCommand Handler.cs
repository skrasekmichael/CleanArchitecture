using TeamUp.Application.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Users.CompleteRegistration;

internal sealed class CompleteRegistrationCommandHandler : ICommandHandler<CompleteRegistrationCommand, Result>
{
	private readonly IUserRepository _userRepository;
	private readonly IPasswordService _passwordService;
	private readonly IUnitOfWork _unitOfWork;

	public CompleteRegistrationCommandHandler(IUserRepository userRepository, IPasswordService passwordService, IUnitOfWork unitOfWork)
	{
		_userRepository = userRepository;
		_passwordService = passwordService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(CompleteRegistrationCommand command, CancellationToken ct)
	{
		var user = await _userRepository.GetUserByIdAsync(command.UserId, ct);
		return await user
			.EnsureNotNull(UserErrors.UserNotFound)
			.Then(user => user.CompleteGeneratedRegistration(_passwordService.HashPassword(command.Password)))
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
