using Microsoft.EntityFrameworkCore;

namespace TeamUp.Tests.EndToEnd.EndpointTests.Teams;

public sealed class CreateTeamTests(AppFixture app) : TeamTests(app)
{
	public const string URL = "/api/v1/teams";

	[Fact]
	public async Task CreateTeam_Should_CreateNewTeamInDatabase_WithOneTeamOwner()
	{
		//arrange
		var user = UserGenerators.User.Generate();
		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		var createTeamRequest = new CreateTeamRequest
		{
			Name = TeamGenerators.GenerateValidTeamName()
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, createTeamRequest);

		//assert
		response.Should().Be201Created();

		var teamId = await response.ReadFromJsonAsync<TeamId>();
		teamId.ShouldNotBeNull();

		var team = await UseDbContextAsync(dbContext =>
		{
			return dbContext.Teams
				.AsSplitQuery()
				.Include(team => team.Members)
				.Include(team => team.EventTypes)
				.FirstOrDefaultAsync(team => team.Id == teamId);
		});

		team.ShouldNotBeNull();
		team.Name.Should().Be(createTeamRequest.Name);
		team.EventTypes.Should().BeEmpty();

		var tm = team.Members[0];
		tm.UserId.Should().Be(user.Id);
		tm.TeamId.Should().Be(teamId);
		tm.Nickname.Should().Be(user.Name);
		tm.Role.Should().Be(TeamRole.Owner);
	}
}
