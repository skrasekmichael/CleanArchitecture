namespace TeamUp.Tests.EndToEnd.EndpointTests.Teams;

public sealed class GetTeamTests(AppFixture app) : TeamTests(app)
{
	public static string GetUrl(TeamId teamId) => GetUrl(teamId.Value);
	public static string GetUrl(Guid teamId) => $"/api/v1/teams/{teamId}";

	[Fact]
	public async Task GetTeam_ThatDoesNotExist_Should_ResultInNotFound()
	{
		//arrange
		var user = UserGenerators.User.Generate();
		var teamId = Guid.NewGuid();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync(CancellationToken);
		});
		Authenticate(user);

		//act
		var response = await Client.GetAsync(GetUrl(teamId), CancellationToken);

		//assert
		response.ShouldBe404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task GetTeam_AsTeamMember_Should_ReturnTeam(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(initiatorUser, initiatorRole, members)
			.WithEventTypes(5)
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.GetAsync(GetUrl(team.Id), CancellationToken);

		//assert
		response.ShouldBe200OK();

		var teamResponse = await response.ReadFromJsonAsync<TeamResponse>();
		team.ShouldHaveSameValuesAs(teamResponse);
	}

	[Fact]
	public async Task GetTeam_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.GetAsync(GetUrl(team.Id), CancellationToken);

		//assert
		response.ShouldBe403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}
}
