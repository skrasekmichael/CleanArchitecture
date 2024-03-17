using TeamUp.Application.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.DomainServices;

namespace TeamUp.Application.Events.RemoveEvent;

internal sealed class RemoveEventCommandHandler : ICommandHandler<RemoveEventCommand, Result>
{
	private readonly IEventDomainService _eventDomainService;
	private readonly IUnitOfWork _unitOfWork;

	public RemoveEventCommandHandler(IEventDomainService eventDomainService, IUnitOfWork unitOfWork)
	{
		_eventDomainService = eventDomainService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(RemoveEventCommand command, CancellationToken ct)
	{
		return await _eventDomainService
			.DeleteEventAsync(command.InitiatorId, command.TeamId, command.EventId, ct)
			.TapAsync(() => _unitOfWork.SaveChangesAsync());
	}
}
