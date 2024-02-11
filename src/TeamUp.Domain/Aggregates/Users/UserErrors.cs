using TeamUp.Common;

namespace TeamUp.Domain.Aggregates.Users;

public static class UserErrors
{
	public static readonly NotFoundError UserNotFound = NotFoundError.New("User not found.", "Users.NotFound");
	public static readonly NotFoundError AccountNotFound = NotFoundError.New("Account not found.", "Users.AccountNotFound");

	public static readonly ConflictError ConflictingEmail = ConflictError.New("User with this email is already registered.", "Users.ConflictingEmail");
}
