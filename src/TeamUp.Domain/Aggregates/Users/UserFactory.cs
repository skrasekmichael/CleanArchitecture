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
		return await name
			.Ensure(UserRules.UserNameMinSize, UserRules.UserNameMaxSize)
			.ThenAsync(_ => _userRepository.ExistsUserWithConflictingEmailAsync(email, ct))
			.Ensure(conflictingUserExists => conflictingUserExists == false, UserErrors.ConflictingEmail)
			.Then(_ => User.Create(name, email, password))
			.Tap(_userRepository.AddUser);
	}
}
