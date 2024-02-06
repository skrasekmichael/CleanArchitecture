using Microsoft.Extensions.Logging;

using TeamUp.Common.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Invitations.DomainEvents;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Domain.Aggregates.Teams.EventHandlers;

internal sealed class InvitationAcceptedEventHandler : IDomainEventHandler<InvitationAcceptedDomainEvent>
{
	private readonly ITeamRepository _teamRepository;
	private readonly IUserRepository _userRepository;
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly ILogger<InvitationAcceptedEventHandler> _logger;

	public InvitationAcceptedEventHandler(
		ITeamRepository teamRepository,
		IUserRepository userRepository,
		IDateTimeProvider dateTimeProvider,
		ILogger<InvitationAcceptedEventHandler> logger)
	{
		_teamRepository = teamRepository;
		_userRepository = userRepository;
		_dateTimeProvider = dateTimeProvider;
		_logger = logger;
	}

	public async Task Handle(InvitationAcceptedDomainEvent notification, CancellationToken cancellationToken)
	{
		var user = await _userRepository.GetUserByIdAsync(notification.UserId, cancellationToken);
		if (user is null)
		{
			_logger.LogWarning("Accepted invitation for user ({userId}) that doesn't exist.", notification.UserId);
			return;
		}

		var team = await _teamRepository.GetTeamByIdWithTeamMembersAsync(notification.TeamId, cancellationToken);
		if (team is null)
		{
			_logger.LogWarning("Accepted invitation for team ({teamId}) that doesn't exist.", notification.TeamId);
			return;
		}

		team.AddTeamMember(user, _dateTimeProvider);
	}
}
