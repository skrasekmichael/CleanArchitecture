using TeamUp.Common;
using TeamUp.Common.Abstractions;
using TeamUp.Domain.Aggregates.Events;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Domain.DomainServices;

internal sealed class EventDomainService : IEventDomainService
{
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly ITeamRepository _teamRepository;

	public EventDomainService(IDateTimeProvider dateTimeProvider, ITeamRepository teamRepository)
	{
		_dateTimeProvider = dateTimeProvider;
		_teamRepository = teamRepository;
	}

	public async Task<Result<Event>> CreateEventAsync(
		UserId loggedUserId,
		TeamId teamId,
		EventTypeId eventTypeId,
		DateTimeOffset from,
		DateTimeOffset to,
		string description,
		TimeSpan meetTime,
		TimeSpan replyClosingTimeBeforeMeetTime,
		CancellationToken ct = default)
	{
		var team = await _teamRepository.GetTeamByIdAsync(teamId, ct);
		return team
			.EnsureNotNull(TeamErrors.NotMemberOfTeam)
			.Ensure(team => team.EventTypes.Any(type => type.Id == eventTypeId), TeamErrors.EventTypeNotFound)
			.And(team => team.GetTeamMemberByUserId(loggedUserId))
			.Ensure((_, member) => member.Role.CanCreateEvents(), TeamErrors.UnauthorizedToCreateEvents)
			.Then((team, _) => Event.Create(
				eventTypeId,
				team.Id,
				from, to,
				description,
				meetTime,
				replyClosingTimeBeforeMeetTime,
				_dateTimeProvider
			));
	}
}
