using TeamUp.Common.Abstraction;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Domain.Aggregates.Invitations;

internal sealed class InvitationFactory
{
	private readonly IDateTimeProvider _dateTimeProvider;

	public InvitationFactory(IDateTimeProvider dateTimeProvider)
	{
		_dateTimeProvider = dateTimeProvider;
	}

	public Invitation CreateInvitation(UserId userId, TeamId teamId) =>
		new(InvitationId.New(), userId, teamId, _dateTimeProvider.UtcNow);
}
