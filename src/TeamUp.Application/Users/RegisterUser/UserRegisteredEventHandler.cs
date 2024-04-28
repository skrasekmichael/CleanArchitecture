using TeamUp.Common.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Users.IntegrationEvents;

namespace TeamUp.Application.Users.Register;

internal sealed class UserRegisteredEventHandler : IIntegrationEventHandler<UserRegisteredEvent>
{
	private readonly IEmailSender _emailSender;

	public UserRegisteredEventHandler(IEmailSender emailSender)
	{
		_emailSender = emailSender;
	}

	public Task Handle(UserRegisteredEvent integrationEvent, CancellationToken ct)
	{
		return _emailSender.SendEmailAsync(
			email: integrationEvent.Email,
			subject: "Successful registration",
			message: $"You need to activate at /users/{integrationEvent.UserId}/activate your account to finalize your registration.", ct);
	}
}
