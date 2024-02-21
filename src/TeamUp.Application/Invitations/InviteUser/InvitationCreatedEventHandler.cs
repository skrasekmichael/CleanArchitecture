using TeamUp.Common.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Invitations.IntegrationEvents;

namespace TeamUp.Application.Invitations.InviteUser;

internal sealed class InvitationCreatedEventHandler : IIntegrationEventHandler<InvitationCreatedEvent>
{
	private readonly IEmailSender _emailSender;

	public InvitationCreatedEventHandler(IEmailSender emailSender)
	{
		_emailSender = emailSender;
	}

	public async Task Handle(InvitationCreatedEvent integrationEvent, CancellationToken ct)
	{
		var subject = "TeamUp - invitation";
		var message = $"You have been invited to team {integrationEvent.TeamName}.";
		await _emailSender.SendEmailAsync(integrationEvent.Email, subject, message, ct);
	}
}
