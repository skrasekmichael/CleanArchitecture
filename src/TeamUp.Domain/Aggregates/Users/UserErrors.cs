using TeamUp.Common;
using TeamUp.Contracts.Users;

namespace TeamUp.Domain.Aggregates.Users;

public static class UserErrors
{
	public static readonly ValidationError UserNameMinSize = ValidationError.New($"Name must be atleast {UserConstants.USERNAME_MIN_SIZE} characters long.", "Users.NameMinSize");
	public static readonly ValidationError UserNameMaxSize = ValidationError.New($"Name must be shorter than {UserConstants.USERNAME_MAX_SIZE} characters.", "Users.NameMaxSize");

	public static readonly NotFoundError UserNotFound = NotFoundError.New("User not found.", "Users.NotFound");
	public static readonly NotFoundError AccountNotFound = NotFoundError.New("Account not found.", "Users.AccountNotFound");

	public static readonly ConflictError ConflictingEmail = ConflictError.New("User with this email is already registered.", "Users.ConflictingEmail");
}
