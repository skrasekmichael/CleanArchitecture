using TeamUp.Common;
using TeamUp.Common.Abstractions;
using TeamUp.Contracts.Events;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;
using TeamUp.Domain.Aggregates.Events;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Domain.DomainServices;

internal sealed class EventDomainService : IEventDomainService
{
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly ITeamRepository _teamRepository;
	private readonly IEventRepository _eventRepository;

	public EventDomainService(IDateTimeProvider dateTimeProvider, ITeamRepository teamRepository, IEventRepository eventRepository)
	{
		_dateTimeProvider = dateTimeProvider;
		_teamRepository = teamRepository;
		_eventRepository = eventRepository;
	}

	public async Task<Result<EventId>> CreateEventAsync(
		UserId initiatorId,
		TeamId teamId,
		EventTypeId eventTypeId,
		DateTime fromUtc,
		DateTime toUtc,
		string description,
		TimeSpan meetTime,
		TimeSpan replyClosingTimeBeforeMeetTime,
		CancellationToken ct = default)
	{
		var team = await _teamRepository.GetTeamByIdAsync(teamId, ct);
		return team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Ensure(team => team.EventTypes.Any(type => type.Id == eventTypeId), TeamErrors.EventTypeNotFound)
			.And(team => team.GetTeamMemberByUserId(initiatorId))
			.Ensure((_, member) => member.Role.CanCreateEvents(), TeamErrors.UnauthorizedToCreateEvents)
			.Then((team, _) => Event.Create(
				eventTypeId,
				team.Id,
				fromUtc,
				toUtc,
				description,
				meetTime,
				replyClosingTimeBeforeMeetTime,
				_dateTimeProvider
			))
			.Tap(_eventRepository.AddEvent)
			.Then(@event => @event.Id);
	}
}
