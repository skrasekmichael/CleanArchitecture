namespace TeamUp.EndToEndTests.EndpointTests.Teams;

public abstract class BaseTeamTests : BaseEndpointTests
{
	public BaseTeamTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	protected static bool TeamContainsMemberWithSameRole(Team team, TeamMember member) => member.Role == team.Members.FirstOrDefault(m => m.Id == member.Id)?.Role;
}
