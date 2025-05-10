using TeamUp.Common.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Users.IntegrationEvents;

namespace TeamUp.Application.Users.RegisterUser;

internal sealed class UserRegisteredEventHandler : IIntegrationEventHandler<UserRegisteredEvent>
{
	private readonly IEmailSender _emailSender;
	private readonly IClientUrlGenerator _urlGenerator;

	public UserRegisteredEventHandler(IEmailSender emailSender, IClientUrlGenerator urlGenerator)
	{
		_emailSender = emailSender;
		_urlGenerator = urlGenerator;
	}

	public Task Handle(UserRegisteredEvent integrationEvent, CancellationToken ct)
	{
		return _emailSender.SendEmailAsync(
			email: integrationEvent.Email,
			subject: "Successful Registration",
			message: $"You need to activate at your account at {_urlGenerator.GetActivationUrl(integrationEvent.UserId)} to finalize your registration.", ct);
	}
}
