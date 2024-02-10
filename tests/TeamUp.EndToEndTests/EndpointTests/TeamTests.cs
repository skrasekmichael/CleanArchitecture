using Microsoft.EntityFrameworkCore;

using TeamUp.Contracts.Teams;

namespace TeamUp.EndToEndTests.EndpointTests;

public sealed class TeamTests : BaseEndpointTests
{
	public TeamTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	[Fact]
	public async Task CreateTeam_Should_CreateNewTeamInDatabase_WithOneTeamMember()
	{
		//arrange
		var user = UserGenerator.ActivatedUser.Generate();
		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		var createTeamRequest = new CreateTeamRequest
		{
			Name = TeamGenerator.GenerateValidTeamName()
		};

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/teams", createTeamRequest);

		//assert
		response.Should().Be201Created();

		var teamId = await response.Content.ReadFromJsonAsync<TeamId>();
		teamId.ShouldNotBeNull();

		var team = await UseDbContextAsync(dbContext =>
			dbContext.Teams
				.Include(team => team.Members)
				.Include(team => team.EventTypes)
				.FirstOrDefaultAsync(team => team.Id == teamId));

		team.ShouldNotBeNull();
		team.Name.Should().Be(createTeamRequest.Name);
		team.EventTypes.Should().BeEmpty();

		var tm = team.Members[0];
		tm.UserId.Should().Be(user.Id);
		tm.TeamId.Should().Be(teamId);
		tm.Nickname.Should().Be(user.Name);
		tm.Role.Should().Be(TeamRole.Owner);
	}

	[Fact]
	public async Task GetTeam_AsTeamOwner_Should_ReturnTeamFromDatabase()
	{
		//arrange
		var user = UserGenerator.ActivatedUser.Generate();
		var team = TeamGenerator.GenerateTeamWithOwner(user);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		//act
		var response = await Client.GetAsync($"/api/v1/teams/{team.Id.Value}");

		//assert
		response.Should().Be200Ok();

		var teamResponse = await response.Content.ReadFromJsonAsync<TeamResponse>();
		teamResponse.ShouldNotBeNull();
		team.Should().BeEquivalentTo(teamResponse, options =>
		{
			return options
				.ExcludingMissingMembers()
				.IgnoringCyclicReferences();
		});
	}
}
