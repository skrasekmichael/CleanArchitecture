using TeamUp.Common;
using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Domain.DomainServices;

internal sealed class InvitationDomainService : IInvitationDomainService
{
	private readonly IUserRepository _userRepository;
	private readonly ITeamRepository _teamRepository;
	private readonly InvitationFactory _invitationFactory;

	public InvitationDomainService(
		IUserRepository userRepository,
		ITeamRepository teamRepository,
		InvitationFactory invitationFactory)
	{
		_userRepository = userRepository;
		_teamRepository = teamRepository;
		_invitationFactory = invitationFactory;
	}

	public async Task<Result<Invitation>> InviteUserAsync(UserId loggedUserId, TeamId teamId, string email, CancellationToken ct = default)
	{
		var team = await _teamRepository.GetTeamByIdAsync(teamId, ct);
		var member = team?.Members.FirstOrDefault(member => member.UserId == loggedUserId);
		if (member is null)
			return AuthorizationError.New("Not member of the team.");

		if (!member.Role.CanInviteTeamMembers())
			return AuthorizationError.New("Can't invite team members.");

		var user = await _userRepository.GetUserByEmailAsync(email, ct);
		user ??= User.Generate(email);

		return _invitationFactory.CreateInvitation(user.Id, teamId);
	}
}
