using TeamUp.Application.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.DomainServices;

namespace TeamUp.Application.Invitations.RemoveInvitation;

internal sealed class RemoveInvitationCommandHandler : ICommandHandler<RemoveInvitationCommand, Result>
{
	private readonly IInvitationDomainService _invitationDomainService;
	private readonly IUnitOfWork _unitOfWork;

	public RemoveInvitationCommandHandler(IInvitationDomainService invitationDomainService, IUnitOfWork unitOfWork)
	{
		_invitationDomainService = invitationDomainService;
		_unitOfWork = unitOfWork;
	}

	public Task<Result> Handle(RemoveInvitationCommand command, CancellationToken ct)
	{
		return _invitationDomainService
			.RemoveInvitationAsync(command.InitiatorId, command.InvitationId, ct)
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
