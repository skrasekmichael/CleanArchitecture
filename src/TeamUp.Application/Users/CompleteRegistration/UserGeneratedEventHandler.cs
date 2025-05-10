using TeamUp.Common.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Users.IntegrationEvents;

namespace TeamUp.Application.Users.CompleteRegistration;

internal sealed class UserGeneratedEventHandler : IIntegrationEventHandler<UserGeneratedEvent>
{
	private readonly IEmailSender _emailSender;
	private readonly IClientUrlGenerator _urlGenerator;

	public UserGeneratedEventHandler(IEmailSender emailSender, IClientUrlGenerator urlGenerator)
	{
		_emailSender = emailSender;
		_urlGenerator = urlGenerator;
	}

	public Task Handle(UserGeneratedEvent integrationEvent, CancellationToken ct)
	{
		return _emailSender.SendEmailAsync(
			email: integrationEvent.Email,
			subject: "Account has been created",
			message: $"You need to finalize your registration at {_urlGenerator.GetCompleteAccountRegistrationUrl(integrationEvent.UserId)}.", ct);
	}
}
