using TeamUp.Common.Abstractions;
using TeamUp.Contracts.Invitations;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Domain.Aggregates.Invitations;

internal sealed class InvitationFactory
{
	private readonly IInvitationRepository _invitationRepository;
	private readonly IDateTimeProvider _dateTimeProvider;

	public InvitationFactory(IInvitationRepository invitationRepository, IDateTimeProvider dateTimeProvider)
	{
		_invitationRepository = invitationRepository;
		_dateTimeProvider = dateTimeProvider;
	}

	public async Task<Result<Invitation>> CreateInvitationAsync(UserId userId, TeamId teamId, CancellationToken ct)
	{
		if (await _invitationRepository.ExistsInvitationForUserToTeamAsync(userId, teamId, ct))
		{
			return InvitationErrors.UserIsAlreadyInvited;
		}

		return new Invitation(InvitationId.New(), userId, teamId, _dateTimeProvider.UtcNow);
	}
}
