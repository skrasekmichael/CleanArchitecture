namespace TeamUp.Tests.EndToEnd.EndpointTests.Teams;

public abstract class TeamTests(AppFixture app) : BaseEndpointTests(app)
{
	protected static bool TeamContainsMemberWithSameRole(Team team, TeamMember member) => member.Role == team.Members.FirstOrDefault(m => m.Id == member.Id)?.Role;
}
