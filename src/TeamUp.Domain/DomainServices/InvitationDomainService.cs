using TeamUp.Common.Abstractions;
using TeamUp.Contracts.Invitations;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;
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
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly UserFactory _userFactory;

	public InvitationDomainService(
		IUserRepository userRepository,
		ITeamRepository teamRepository,
		IInvitationRepository invitationRepository,
		IIntegrationEventManager integrationEventManager,
		InvitationFactory invitationFactory,
		IDateTimeProvider dateTimeProvider,
		UserFactory userFactory)
	{
		_userRepository = userRepository;
		_teamRepository = teamRepository;
		_invitationRepository = invitationRepository;
		_integrationEventManager = integrationEventManager;
		_invitationFactory = invitationFactory;
		_dateTimeProvider = dateTimeProvider;
		_userFactory = userFactory;
	}

	public async Task<Result> AcceptInvitationAsync(UserId initiatorId, InvitationId invitationId, CancellationToken ct = default)
	{
		var invitation = await _invitationRepository.GetInvitationByIdAsync(invitationId, ct);
		return await invitation
			.EnsureNotNull(InvitationErrors.InvitationNotFound)
			.Ensure(invitation => invitation.RecipientId == initiatorId, InvitationErrors.UnauthorizedToAcceptInvitation)
			.Ensure(invitation => !invitation.HasExpired(_dateTimeProvider.UtcNow), InvitationErrors.InvitationExpired)
			.AndAsync(invitation => _teamRepository.GetTeamByIdAsync(invitation.TeamId, ct))
			.EnsureSecondNotNull(TeamErrors.TeamNotFound)
			.ThenAsync(async (invitation, team) =>
			{
				return await team
					.Ensure(TeamRules.TeamHasNotReachedCapacity)
					.ThenAsync(_ => _userRepository.GetUserByIdAsync(invitation.RecipientId))
					.EnsureNotNull(UserErrors.AccountNotFound)
					.Tap(user =>
					{
						team.AddTeamMember(user, _dateTimeProvider);
						_invitationRepository.RemoveInvitation(invitation);
					});
			})
			.ToResultAsync();
	}

	public async Task<Result<InvitationId>> InviteUserAsync(UserId initiatorId, TeamId teamId, string email, CancellationToken ct = default)
	{
		var team = await _teamRepository.GetTeamByIdAsync(teamId, ct);
		return await team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Ensure(TeamRules.TeamHasNotReachedCapacity)
			.And(team => team.GetTeamMemberByUserId(initiatorId))
			.Ensure((_, member) => member.Role.CanInviteTeamMembers(), TeamErrors.UnauthorizedToInviteTeamMembers)
			.Then((team, _) => team)
			.AndAsync(team => _userRepository.GetUserByEmailAsync(email, ct))
			.Ensure(TeamRules.InvitedUserIsNotTeamMember)
			.Then((team, user) =>
			{
				if (user is null)
				{
					var generatedUser = _userFactory.GenerateAndAddUser(email);
					return (team, generatedUser);
				}

				return (team, user);
			})
			.Tap((team, user) => _integrationEventManager.AddIntegrationEvent(new InvitationCreatedEvent(user.Id, user.Email, team.Id, team.Name)))
			.ThenAsync((team, user) => _invitationFactory.CreateInvitationAsync(user.Id, team.Id, ct))
			.Tap(_invitationRepository.AddInvitation)
			.Then(invitation => invitation.Id);
	}

	public async Task<Result> RemoveInvitationAsync(UserId initiatorId, InvitationId invitationId, CancellationToken ct = default)
	{
		var invitation = await _invitationRepository.GetInvitationByIdAsync(invitationId, ct);
		return await invitation
			.EnsureNotNull(InvitationErrors.InvitationNotFound)
			.ThenAsync(async invitation =>
			{
				if (invitation.RecipientId == initiatorId)
					return invitation;

				var team = await _teamRepository.GetTeamByIdAsync(invitation.TeamId, ct);
				return team
					.EnsureNotNull(TeamErrors.NotMemberOfTeam)
					.Then(team => team.GetTeamMemberByUserId(initiatorId))
					.Ensure(member => member.Role.CanInviteTeamMembers(), TeamErrors.UnauthorizedToCancelInvitations)
					.Then(_ => invitation);
			})
			.Tap(_invitationRepository.RemoveInvitation)
			.ToResultAsync();
	}
}
