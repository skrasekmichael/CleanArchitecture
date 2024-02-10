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

	public async Task Handle(InvitationAcceptedDomainEvent domainEvent, CancellationToken ct)
	{
		var user = await _userRepository.GetUserByIdAsync(domainEvent.UserId, ct);
		if (user is null)
		{
			_logger.LogWarning("Accepted invitation for user ({userId}) that doesn't exist.", domainEvent.UserId);
			return;
		}

		var team = await _teamRepository.GetTeamByIdAsync(domainEvent.TeamId, ct);
		if (team is null)
		{
			_logger.LogWarning("Accepted invitation for team ({teamId}) that doesn't exist.", domainEvent.TeamId);
			return;
		}

		team.AddTeamMember(user, _dateTimeProvider);
	}
}
