using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Events;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Events.CreateEvent;

public sealed record CreateEventCommand(
	UserId InitiatorId,
	TeamId TeamId,
	EventTypeId EventTypeId,
	DateTime FromUtc,
	DateTime ToUtc,
	string Description,
	TimeSpan MeetTime,
	TimeSpan ReplyClosingTimeBeforeMeetTime
) : ICommand<Result<EventId>>;
