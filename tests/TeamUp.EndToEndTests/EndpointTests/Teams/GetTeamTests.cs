using TeamUp.Contracts.Teams;

namespace TeamUp.EndToEndTests.EndpointTests.Teams;

public sealed class GetTeamTests : BaseTeamTests
{
	public GetTeamTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	[Fact]
	public async Task GetTeam_ThatDoesNotExist_Should_ResultInNotFound()
	{
		//arrange
		var user = UserGenerator.ActivatedUser.Generate();
		var teamId = F.Random.Guid();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		//act
		var response = await Client.GetAsync($"/api/v1/teams/{teamId}");

		//assert
		response.Should().Be404NotFound();

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
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(initiatorUser, initiatorRole, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.GetAsync($"/api/v1/teams/{team.Id.Value}");

		//assert
		response.Should().Be200Ok();

		var teamResponse = await response.ReadFromJsonAsync<TeamResponse>();
		teamResponse.ShouldNotBeNull();
		team.Should().BeEquivalentTo(teamResponse, options =>
		{
			return options
				.ExcludingMissingMembers()
				.IgnoringCyclicReferences();
		});
	}

	[Fact]
	public async Task GetTeam_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(owner, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.GetAsync($"/api/v1/teams/{team.Id.Value}");

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}
}
