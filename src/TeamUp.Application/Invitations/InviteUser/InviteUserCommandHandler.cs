using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Invitations;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.DomainServices;

namespace TeamUp.Application.Invitations.InviteUser;

internal sealed class InviteUserCommandHandler : ICommandHandler<InviteUserCommand, Result<InvitationId>>
{
	private readonly IInvitationDomainService _invitationDomainService;
	private readonly IUnitOfWork _unitOfWork;

	public InviteUserCommandHandler(IInvitationDomainService invitationDomainService, IUnitOfWork unitOfWork)
	{
		_invitationDomainService = invitationDomainService;
		_unitOfWork = unitOfWork;
	}

	public Task<Result<InvitationId>> Handle(InviteUserCommand request, CancellationToken ct)
	{
		return _invitationDomainService
			.InviteUserAsync(request.InitiatorId, request.TeamId, request.Email, ct)
			.TapAsync(_ => _unitOfWork.SaveChangesAsync(ct));
	}
}
