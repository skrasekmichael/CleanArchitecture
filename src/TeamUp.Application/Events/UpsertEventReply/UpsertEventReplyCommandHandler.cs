using TeamUp.Application.Abstractions;
using TeamUp.Common.Abstractions;
using TeamUp.Contracts.Events;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Events;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Application.Events.UpsertEventReply;

internal sealed class UpsertEventReplyCommandHandler : ICommandHandler<UpsertEventReplyCommand, Result>
{
	private readonly IEventRepository _eventRepository;
	private readonly ITeamRepository _teamRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IDateTimeProvider _dateTimeProvider;

	public UpsertEventReplyCommandHandler(IEventRepository eventRepository, ITeamRepository teamRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
	{
		_eventRepository = eventRepository;
		_teamRepository = teamRepository;
		_unitOfWork = unitOfWork;
		_dateTimeProvider = dateTimeProvider;
	}

	public async Task<Result> HandleAsync(UpsertEventReplyCommand command, CancellationToken ct)
	{
		var team = await _teamRepository.GetTeamByIdAsync(command.TeamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.GetTeamMemberByUserId(command.InitiatorId))
			.AndAsync(_ => _eventRepository.GetEventByIdAsync(command.EventId, ct))
			.EnsureSecondNotNull(EventErrors.EventNotFound)
			.Ensure((_, @event) => @event.TeamId == command.TeamId, EventErrors.EventNotFound)
			.And((_, _) => MapRequestToReply(command))
			.Then((member, @event, reply) => @event.SetMemberResponse(_dateTimeProvider, member.Id, reply))
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}

	private static Result<EventReply> MapRequestToReply(UpsertEventReplyCommand command) => command.ReplyType switch
	{
		ReplyType.Yes => EventReply.Yes(),
		ReplyType.Maybe => EventReply.Maybe(command.Message),
		ReplyType.Delay => EventReply.Delay(command.Message),
		ReplyType.No => EventReply.No(command.Message),
		_ => new InternalError("InternalErrors.MissingSwitchCase", $"{nameof(MapRequestToReply)} does not implement case for type [{command.ReplyType}]")
	};
}
