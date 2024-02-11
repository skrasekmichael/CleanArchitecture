using TeamUp.Common;

namespace TeamUp.Domain.Aggregates.Users;

public sealed class UserFactory
{
	private readonly IUserRepository _userRepository;

	public UserFactory(IUserRepository userRepository)
	{
		_userRepository = userRepository;
	}

	public async Task<Result<User>> CreateAndAddUserAsync(string name, string email, Password password, CancellationToken ct = default)
	{
		if (await _userRepository.ExistsUserWithConflictingEmailAsync(email, ct))
			return UserErrors.ConflictingEmail;

		var user = User.Create(name, email, password);
		_userRepository.AddUser(user);

		return user;
	}
}
