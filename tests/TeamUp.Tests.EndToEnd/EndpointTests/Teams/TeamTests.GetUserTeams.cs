namespace TeamUp.Tests.EndToEnd.EndpointTests.Teams;

public sealed class GetUserTeamsTests(AppFixture app) : TeamTests(app)
{
	public const string URL = "/api/v1/teams";

	[Fact]
	public async Task GetUserTeams_WhenNotMemberOfTeam_Should_ReturnEmptyList()
	{
		//arrange
		var user = UserGenerators.User.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(user);

		//act
		var response = await Client.GetAsync(URL, CancellationToken);

		//assert
		response.ShouldBe200OK();

		var teams = await response.ReadFromJsonAsync<List<TeamSlimResponse>>();
		teams.ShouldBeEmpty();
	}

	[Fact]
	public async Task GetUserTeams_Should_ReturnTeams_ThatUserIsMemberOf()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);

		var memberTeams = new TeamRole[] { TeamRole.Owner, TeamRole.Admin, TeamRole.Coordinator, TeamRole.Member }
			.Select(role => TeamGenerators.Team.WithMembers(initiatorUser, role, members).Generate())
			.ToList();
		var expectedTeams = memberTeams.Select(team => new TeamSlimResponse
		{
			TeamId = team.Id,
			Name = team.Name,
			NumberOfTeamMembers = team.NumberOfMembers
		});

		var otherTeam = TeamGenerators.Team.WithOneOwner(members.First());

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(otherTeam);
			dbContext.Teams.AddRange(memberTeams);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.GetAsync(URL, CancellationToken);

		//assert
		response.ShouldBe200OK();

		var teams = await response.ReadFromJsonAsync<List<TeamSlimResponse>>();
		teams.ShouldNotBeNull();
		teams.OrderBy(t => t.TeamId).ShouldHaveSameValuesAs(expectedTeams.OrderBy(t => t.TeamId));
	}
}
