using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Users;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Users.RegisterUser;

internal sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Result<UserId>>
{
	private readonly UserFactory _userFactory;
	private readonly IPasswordService _passwordService;
	private readonly IUnitOfWork _unitOfWork;

	public RegisterUserCommandHandler(UserFactory userFactory, IPasswordService passwordService, IUnitOfWork unitOfWork)
	{
		_userFactory = userFactory;
		_passwordService = passwordService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<UserId>> Handle(RegisterUserCommand command, CancellationToken ct)
	{
		var password = _passwordService.HashPassword(command.Password);
		var user = await _userFactory.CreateAndAddUserAsync(command.Name, command.Email, password, ct);

		return await user
			.Then(user => user.Id)
			.TapAsync(_ => _unitOfWork.SaveChangesAsync(ct));
	}
}
