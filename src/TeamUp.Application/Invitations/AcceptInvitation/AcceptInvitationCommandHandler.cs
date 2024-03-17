using TeamUp.Application.Abstractions;
using TeamUp.Common.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Invitations;

namespace TeamUp.Application.Invitations.AcceptInvitation;

internal sealed class AcceptInvitationCommandHandler : ICommandHandler<AcceptInvitationCommand, Result>
{
	private readonly IInvitationRepository _invitationRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IDateTimeProvider _dateTimeProvider;

	public AcceptInvitationCommandHandler(IInvitationRepository invitationRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
	{
		_invitationRepository = invitationRepository;
		_unitOfWork = unitOfWork;
		_dateTimeProvider = dateTimeProvider;
	}

	public async Task<Result> Handle(AcceptInvitationCommand command, CancellationToken ct)
	{
		var invitation = await _invitationRepository.GetInvitationByIdAsync(command.InvitationId, ct);
		return await invitation
			.EnsureNotNull(InvitationErrors.InvitationNotFound)
			.Then(invitation => invitation.Accept(command.InitiatorId, _dateTimeProvider))
			.TapAsync(() => _unitOfWork.SaveChangesAsync(ct));
	}
}
