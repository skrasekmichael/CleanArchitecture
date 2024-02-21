using TeamUp.Common;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.Aggregates.Invitations.IntegrationEvents;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Domain.DomainServices;

internal sealed class InvitationDomainService : IInvitationDomainService
{
	private readonly IUserRepository _userRepository;
	private readonly ITeamRepository _teamRepository;
	private readonly IInvitationRepository _invitationRepository;
	private readonly IIntegrationEventManager _integrationEventManager;
	private readonly InvitationFactory _invitationFactory;

	public InvitationDomainService(
		IUserRepository userRepository,
		ITeamRepository teamRepository,
		IInvitationRepository invitationRepository,
		IIntegrationEventManager integrationEventManager,
		InvitationFactory invitationFactory)
	{
		_userRepository = userRepository;
		_teamRepository = teamRepository;
		_invitationRepository = invitationRepository;
		_integrationEventManager = integrationEventManager;
		_invitationFactory = invitationFactory;
	}

	public async Task<Result<InvitationId>> InviteUserAsync(UserId initiatorId, TeamId teamId, string email, CancellationToken ct = default)
	{
		var team = await _teamRepository.GetTeamByIdAsync(teamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.And(team => team.GetTeamMemberByUserId(initiatorId))
			.Ensure((_, member) => member.Role.CanInviteTeamMembers(), TeamErrors.UnauthorizedToInviteTeamMembers)
			.Then((team, _) => team)
			.AndAsync(team => _userRepository.GetUserByEmailAsync(email, ct))
			.Ensure(TeamRules.InvitedUserIsNotTeamMember)
			.Then((team, user) =>
			{
				if (user is null)
				{
					var generatedUser = User.Generate(email);
					_userRepository.AddUser(generatedUser);
					return (team, generatedUser);
				}

				return (team, user);
			})
			.Tap((team, user) => _integrationEventManager.AddIntegrationEvent(new InvitationCreatedEvent(user.Id, user.Email, team.Id, team.Name)))
			.ThenAsync((team, user) => _invitationFactory.CreateInvitationAsync(user.Id, team.Id, ct))
			.Tap(_invitationRepository.AddInvitation)
			.Then(invitation => invitation.Id);
	}
}
