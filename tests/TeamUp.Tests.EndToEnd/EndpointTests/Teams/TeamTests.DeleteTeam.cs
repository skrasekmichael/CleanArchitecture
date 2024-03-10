using Microsoft.EntityFrameworkCore;

using EventResponse = TeamUp.Domain.Aggregates.Events.EventResponse;

namespace TeamUp.Tests.EndToEnd.EndpointTests.Teams;

public sealed class DeleteTeamTests(AppFixture app) : TeamTests(app)
{
	public static string GetUrl(TeamId teamId) => GetUrl(teamId.Value);
	public static string GetUrl(Guid teamId) => $"/api/v1/teams/{teamId}";

	[Fact]
	public async Task DeleteTeam_AsOwner_Should_DeleteTeamInDatabase()
	{
		//arrange
		var owner = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(19);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(owner);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(owner);

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id));

		//assert
		response.Should().Be200Ok();

		await UseDbContextAsync(async dbContext =>
		{
			var deletedTeam = await dbContext.Teams.FindAsync(team.Id);
			deletedTeam.Should().BeNull();

			var members = await dbContext.Set<TeamMember>().ToListAsync();
			members.Should().BeEmpty();

			var eventTypes = await dbContext.Set<EventType>().ToListAsync();
			eventTypes.Should().BeEmpty();

			var events = await dbContext.Events.ToListAsync();
			events.Should().BeEmpty();

			var eventResponses = await dbContext.Set<EventResponse>().ToListAsync();
			eventResponses.Should().BeEmpty();
		});
	}

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	public async Task DeleteTeam_AsAdminOrLower_Should_ResultInForbidden(TeamRole teamRole)
	{
		//arrange
		var owner = UserGenerators.ActivatedUser.Generate();
		var initiatorUser = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(18);
		var team = TeamGenerators.Team.WithMembers(owner, members, (initiatorUser, teamRole)).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id));

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToDeleteTeam);
	}

	[Fact]
	public async Task DeleteTeam_ThatDoesNotExist_Should_ResultInNotFound()
	{
		//arrange
		var user = UserGenerators.ActivatedUser.Generate();
		var teamId = Guid.NewGuid();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});
		Authenticate(user);

		//act
		var response = await Client.DeleteAsync(GetUrl(teamId));

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task DeleteTeam_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerators.ActivatedUser.Generate();
		var initiatorUser = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(19);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id));

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}
}
