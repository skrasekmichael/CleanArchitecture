using TeamUp.Contracts.Users;

namespace TeamUp.Domain.Abstractions;

public interface IClientUrlGenerator
{
	public string GetActivationUrl(UserId userId);
	public string GetCompleteAccountRegistrationUrl(UserId userId);
}
