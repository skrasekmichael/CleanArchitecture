using TeamUp.Common;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Domain.SeedWork;

namespace TeamUp.Application.Users.Register;

internal sealed class RegisterCommandHandler : ICommandHandler<RegisterUserCommand, Result<UserId>>
{
	private readonly IUserRepository _userRepository;
	private readonly IPasswordService _passwordService;
	private readonly IUnitOfWork _unitOfWork;

	public RegisterCommandHandler(IUserRepository userRepository, IPasswordService passwordService, IUnitOfWork unitOfWork)
	{
		_userRepository = userRepository;
		_passwordService = passwordService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<UserId>> Handle(RegisterUserCommand request, CancellationToken ct)
	{
		var password = _passwordService.HashPassword(request.Password);
		var user = User.Create(request.Name, request.Email, password);

		_userRepository.AddUser(user);
		await _unitOfWork.SaveChangesAsync(ct);

		return user.Id;
	}
}
