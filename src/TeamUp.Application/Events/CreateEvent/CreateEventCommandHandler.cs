using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Events;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.DomainServices;

namespace TeamUp.Application.Events.CreateEvent;

internal sealed class CreateEventCommandHandler : ICommandHandler<CreateEventCommand, Result<EventId>>
{
	private readonly IEventDomainService _eventDomainService;
	private readonly IUnitOfWork _unitOfWork;

	public CreateEventCommandHandler(IEventDomainService eventDomainService, IUnitOfWork unitOfWork)
	{
		_eventDomainService = eventDomainService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<EventId>> Handle(CreateEventCommand command, CancellationToken ct)
	{
		return await _eventDomainService
			.CreateEventAsync(
				initiatorId: command.InitiatorId,
				teamId: command.TeamId,
				eventTypeId: command.EventTypeId,
				fromUtc: command.FromUtc,
				toUtc: command.ToUtc,
				description: command.Description,
				meetTime: command.MeetTime,
				replyClosingTimeBeforeMeetTime: command.ReplyClosingTimeBeforeMeetTime,
				ct: ct)
			.TapAsync(_ => _unitOfWork.SaveChangesAsync(ct));
	}
}
