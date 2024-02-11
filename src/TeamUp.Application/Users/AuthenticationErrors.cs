using TeamUp.Common;

namespace TeamUp.Application.Users;

public static class AuthenticationErrors
{
	public static readonly AuthenticationError InvalidCredentials = AuthenticationError.New("Invalid Credentials.", "Auth.InvalidCredentials");
	public static readonly AuthenticationError NotActivatedAccount = AuthenticationError.New("Account is not activated.", "Auth.NotActivatedAccount");
}
