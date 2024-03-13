using Microsoft.EntityFrameworkCore;

namespace TeamUp.Tests.EndToEnd.EndpointTests.Teams;

public sealed class DeleteTeamTests(AppFixture app) : TeamTests(app)
{
	public static string GetUrl(TeamId teamId) => GetUrl(teamId.Value);
	public static string GetUrl(Guid teamId) => $"/api/v1/teams/{teamId}";

	[Fact]
	public async Task DeleteTeam_AsOwner_Should_DeleteTeamInDatabase()
	{
		//arrange
		var users = UserGenerators.ActivatedUser.Generate(80);
		var teams = TeamGenerators.Team
			.WithRandomMembers(25, users)
			.WithEventTypes(5)
			.Generate(4);

		var teamEvents = teams.Select(team =>
		{
			return EventGenerators.Event
				.ForTeam(team.Id)
				.WithEventType(team.EventTypes[0].Id)
				.WithRandomEventResponses(team.Members)
				.Generate(15);
		}).ToList();
		var events = teamEvents.SelectMany(events => events);

		var targetTeamIndex = F.Random.Int(0, teams.Count - 1);
		var targetTeam = teams[targetTeamIndex];
		var teamOwner = targetTeam.Members.Single(member => member.Role.IsOwner());
		var initiatorUser = users.Single(user => user.Id == teamOwner.UserId);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange(users);
			dbContext.Teams.AddRange(teams);
			dbContext.Events.AddRange(events);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.DeleteAsync(GetUrl(targetTeam.Id));

		//assert
		response.Should().Be200Ok();

		await UseDbContextAsync(async dbContext =>
		{
			//no users deleted
			var notDeletedUsers = await dbContext.Users.ToListAsync();
			notDeletedUsers.Should().BeEquivalentTo(users);

			//only team, team members and event types from target team were deleted
			var notDeletedTeams = await dbContext.Teams
				.Include(team => team.Members)
				.Include(team => team.EventTypes)
				.ToListAsync();
			notDeletedTeams.Should().BeEquivalentTo(teams.Except([targetTeam]));

			//only events and event responses from target team were deleted
			var notDeletedEvents = await dbContext.Events
				.Include(e => e.EventResponses)
				.ToListAsync();
			notDeletedEvents.Should().BeEquivalentTo(events.Except(teamEvents[targetTeamIndex]));
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
