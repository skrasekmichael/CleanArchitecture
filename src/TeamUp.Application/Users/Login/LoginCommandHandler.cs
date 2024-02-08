using TeamUp.Application.Abstractions;
using TeamUp.Common;
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

	public async Task<Result<string>> Handle(LoginCommand request, CancellationToken ct)
	{
		var user = await _userRepository.GetUserByEmailAsync(request.Email, ct);
		if (user is null)
			return AuthenticationError.New("Invalid Credentials");

		if (!_passwordService.VerifyPassword(request.Password, user.Password))
			return AuthenticationError.New("Invalid Credentials");

		if (user.Status != UserStatus.Activated)
			return AuthenticationError.New("Account is not activated");

		return _tokenService.GenerateToken(user);
	}
}
