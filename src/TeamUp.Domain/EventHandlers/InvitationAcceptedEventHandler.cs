using Microsoft.Extensions.Logging;

using TeamUp.Common.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.Aggregates.Invitations.DomainEvents;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Domain.Aggregates.Teams.EventHandlers;

internal sealed class InvitationAcceptedEventHandler : IDomainEventHandler<InvitationAcceptedDomainEvent>
{
	private readonly ITeamRepository _teamRepository;
	private readonly IUserRepository _userRepository;
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly IInvitationRepository _invitationRepository;
	private readonly ILogger<InvitationAcceptedEventHandler> _logger;

	public InvitationAcceptedEventHandler(
		ITeamRepository teamRepository,
		IUserRepository userRepository,
		IDateTimeProvider dateTimeProvider,
		IInvitationRepository invitationRepository,
		ILogger<InvitationAcceptedEventHandler> logger)
	{
		_teamRepository = teamRepository;
		_userRepository = userRepository;
		_dateTimeProvider = dateTimeProvider;
		_invitationRepository = invitationRepository;
		_logger = logger;
	}

	public async Task Handle(InvitationAcceptedDomainEvent domainEvent, CancellationToken ct)
	{
		var user = await _userRepository.GetUserByIdAsync(domainEvent.Invitation.RecipientId, ct);
		if (user is null)
		{
			_logger.LogWarning("Accepted invitation by user ({userId}) that doesn't exist.", domainEvent.Invitation.RecipientId);
			return;
		}

		var team = await _teamRepository.GetTeamByIdAsync(domainEvent.Invitation.TeamId, ct);
		if (team is null)
		{
			_logger.LogWarning("Accepted invitation for team ({teamId}) that doesn't exist.", domainEvent.Invitation.TeamId);
			return;
		}

		team.AddTeamMember(user, _dateTimeProvider);
		_invitationRepository.RemoveInvitation(domainEvent.Invitation);
	}
}
