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
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Then(team => team.GetTeamMemberByUserId(loggedUserId))
			.Ensure(member => member.Role.CanInviteTeamMembers(), TeamErrors.UnauthorizedToInviteTeamMembers)
			.ThenAsync(_ => _userRepository.GetUserByEmailAsync(email, ct))
			.ThenAsync(user => user ?? User.Generate(email))
			.ThenAsync(user => _invitationFactory.CreateInvitation(user.Id, teamId));
	}
}
