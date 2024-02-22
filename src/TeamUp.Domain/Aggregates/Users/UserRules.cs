using TeamUp.Common;
using TeamUp.Contracts.Users;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Domain.Aggregates.Users;

public static class UserRules
{
	static readonly Rule<string> UserNameMinSizeRule = name => name.Length >= UserConstants.USERNAME_MIN_SIZE;
	static readonly Rule<string> UserNameMaxSizeRule = name => name.Length <= UserConstants.USERNAME_MAX_SIZE;

	public static readonly RuleWithError<string> UserNameMinSize = new(UserNameMinSizeRule, UserErrors.UserNameMinSize);
	public static readonly RuleWithError<string> UserNameMaxSize = new(UserNameMaxSizeRule, UserErrors.UserNameMaxSize);

}
