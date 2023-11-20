using TeamUp.Common;
using TeamUp.Common.Abstraction;
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
		var team = await _teamRepository.GetTeamByIdWithTeamMembersAsync(teamId, ct);
		if (team is null)
			return DomainError.New("Team doesn't exist.");

		var member = team.Members.FirstOrDefault(member => member.UserId == loggedUserId);
		if (member is null)
			return AuthorizationError.New("Not member of the team.");

		if (!member.Role.CanCreateEvents())
			return AuthorizationError.New("Can't create events.");

		if (team.EventTypes.All(type => type.Id != eventTypeId))
			return ValidationError.New("Event type doesn't exist.");

		return Event.Create(
			eventTypeId,
			team.Id,
			from, to,
			description,
			meetTime,
			replyClosingTimeBeforeMeetTime,
			_dateTimeProvider
		);
	}
}
