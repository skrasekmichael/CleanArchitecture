using Microsoft.Extensions.Options;
using TeamUp.Contracts.Users;
using TeamUp.Domain.Abstractions;
using TeamUp.Infrastructure.Options;

namespace TeamUp.Infrastructure.Core;

internal sealed class ClientUrlGenerator : IClientUrlGenerator
{
	private readonly ClientOptions _options;

	public ClientUrlGenerator(IOptions<ClientOptions> options)
	{
		_options = options.Value;
	}

	public string GetActivationUrl(UserId userId) =>
		string.Format(_options.ActivateAccountUrl, _options.Url, userId.Value);

	public string GetCompleteAccountRegistrationUrl(UserId userId) =>
		string.Format(_options.CompleteAccountRegistrationUrl, _options.Url, userId.Value);
}
