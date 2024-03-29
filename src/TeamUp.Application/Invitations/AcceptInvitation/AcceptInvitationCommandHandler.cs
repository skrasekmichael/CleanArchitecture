using TeamUp.Application.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.DomainServices;

namespace TeamUp.Application.Invitations.AcceptInvitation;

internal sealed class AcceptInvitationCommandHandler : ICommandHandler<AcceptInvitationCommand, Result>
{
	private readonly IInvitationDomainService _invitationDomainService;
	private readonly IUnitOfWork _unitOfWork;

	public AcceptInvitationCommandHandler(IInvitationDomainService invitationDomainService, IUnitOfWork unitOfWork)
	{
		_invitationDomainService = invitationDomainService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(AcceptInvitationCommand command, CancellationToken ct)
	{
		return await _invitationDomainService
			.AcceptInvitationAsync(command.InitiatorId, command.InvitationId, ct)
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
