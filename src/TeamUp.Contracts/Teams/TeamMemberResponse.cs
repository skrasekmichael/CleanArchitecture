using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Contracts.Teams;

public sealed class TeamMemberResponse
{
	public required TeamMemberId Id { get; init; }
	public required UserId UserId { get; init; }
	public required string Nickname { get; init; }
	public required TeamRole Role { get; init; }
}
