using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Users;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Users.Login;

internal sealed class LoginCommandHandler : ICommandHandler<LoginCommand, Result<string>>
{
	private readonly IUserRepository _userRepository;
	private readonly IPasswordService _passwordService;
	private readonly ITokenService _tokenService;

	public LoginCommandHandler(IUserRepository userRepository, IPasswordService passwordService, ITokenService tokenService)
	{
		_userRepository = userRepository;
		_passwordService = passwordService;
		_tokenService = tokenService;
	}

	public async Task<Result<string>> Handle(LoginCommand command, CancellationToken ct)
	{
		var user = await _userRepository.GetUserByEmailAsync(command.Email, ct);
		return user
			.EnsureNotNull(AuthenticationErrors.InvalidCredentials)
			.Ensure(user => _passwordService.VerifyPassword(command.Password, user.Password), AuthenticationErrors.InvalidCredentials)
			.Ensure(user => user.Status == UserStatus.Activated, AuthenticationErrors.NotActivatedAccount)
			.Then(_tokenService.GenerateToken);
	}
}
