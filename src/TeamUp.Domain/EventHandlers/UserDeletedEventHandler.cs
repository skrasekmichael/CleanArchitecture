using TeamUp.Contracts.Teams;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Domain.EventHandlers;

internal sealed class UserDeletedEventHandler : IDomainEventHandler<UserDeletedDomainEvent>
{
	private readonly IUserRepository _userRepository;
	private readonly ITeamRepository _teamRepository;

	public UserDeletedEventHandler(IUserRepository userRepository, ITeamRepository teamRepository)
	{
		_userRepository = userRepository;
		_teamRepository = teamRepository;
	}

	public async Task Handle(UserDeletedDomainEvent domainEvent, CancellationToken ct)
	{
		var teams = await _teamRepository.GetTeamsByUserIdAsync(domainEvent.User.Id, ct);

		foreach (var team in teams)
		{
			team.GetTeamMemberByUserId(domainEvent.User.Id)
				.Ensure(TeamRules.MemberCanChangeOwnership)
				.Tap(initiator =>
				{
					if (team.Members.Count == 1)
					{
						//remove team if user that is being removed is the only member
						_teamRepository.RemoveTeam(team);
					}
					else
					{
						//change ownership when removing user that is owner of the team
						var newOwner = team.GetHighestNonOwnerTeamMember()!;
						initiator.UpdateRole(TeamRole.Admin);
						newOwner.UpdateRole(TeamRole.Owner);
					}
				});
		}

		_userRepository.RemoveUser(domainEvent.User);
	}
}
