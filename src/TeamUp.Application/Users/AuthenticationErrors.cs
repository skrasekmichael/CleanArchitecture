namespace TeamUp.Application.Users;

public static class AuthenticationErrors
{
	public static readonly AuthenticationError InvalidCredentials = new("Authentication.InvalidCredentials", "Invalid Credentials.");
	public static readonly AuthenticationError NotActivatedAccount = new("Authentication.NotActivatedAccount", "Account is not activated.");
}
