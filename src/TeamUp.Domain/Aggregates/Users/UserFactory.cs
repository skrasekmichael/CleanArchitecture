using TeamUp.Common.Abstractions;
using TeamUp.Contracts.Users;

namespace TeamUp.Domain.Aggregates.Users;

public sealed class UserFactory
{
	private readonly IUserRepository _userRepository;
	private readonly IDateTimeProvider _dateTimeProvider;

	public UserFactory(IUserRepository userRepository, IDateTimeProvider dateTimeProvider)
	{
		_userRepository = userRepository;
		_dateTimeProvider = dateTimeProvider;
	}

	public async Task<Result<User>> CreateAndAddUserAsync(string name, string email, Password password, CancellationToken ct = default)
	{
		return await name
			.Ensure(UserRules.UserNameMinSize, UserRules.UserNameMaxSize)
			.ThenAsync(_ => _userRepository.ExistsUserWithConflictingEmailAsync(email, ct))
			.Ensure(conflictingUserExists => conflictingUserExists == false, UserErrors.ConflictingEmail)
			.Then(_ => new User(UserId.New(), name, email, password, UserStatus.NotActivated, _dateTimeProvider.UtcNow))
			.Tap(_userRepository.AddUser);
	}

	public User GenerateAndAddUser(string email)
	{
		var user = new User(UserId.New(), email, email, new Password(), UserStatus.Generated, _dateTimeProvider.UtcNow);
		_userRepository.AddUser(user);
		return user;
	}
}
