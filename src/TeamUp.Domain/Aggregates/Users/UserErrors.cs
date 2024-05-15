using TeamUp.Contracts.Users;

namespace TeamUp.Domain.Aggregates.Users;

public static class UserErrors
{
	public static readonly ValidationError UserNameMinSize = new("Users.Validation.NameMinSize", $"Name must be atleast {UserConstants.USERNAME_MIN_SIZE} characters long.");
	public static readonly ValidationError UserNameMaxSize = new("Users.Validation.NameMaxSize", $"Name must be shorter than {UserConstants.USERNAME_MAX_SIZE} characters.");

	public static readonly NotFoundError UserNotFound = new("Users.NotFound", "User not found.");
	public static readonly NotFoundError AccountNotFound = new("Users.NotFound.Account", "Account not found.");

	public static readonly ConflictError ConflictingEmail = new("Users.Conflict.Email", "User with this email is already registered.");

	public static readonly DomainError CannotActivateGeneratedAccount = new("Users.Domain.ActivateGeneratedAccount", "Cannot activate generated account.");
	public static readonly DomainError AccountAlreadyActivated = new("Users.Domain.AccountAlreadyActivated", "Account is already activated.");
	public static readonly DomainError CannotCompleteRegistrationOfNonGeneratedAccount = new("Users.Domain.CompleteNonGeneratedAccount", "Cannot complete registration of non-generated account.");
}
