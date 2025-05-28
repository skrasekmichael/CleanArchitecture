using Microsoft.EntityFrameworkCore;

namespace TeamUp.Tests.EndToEnd.EndpointTests.Events;

public sealed class RemoveEventTests(AppFixture app) : EventTests(app)
{
	public static string GetUrl(TeamId teamId, EventId eventId) => GetUrl(teamId.Value, eventId.Value);
	public static string GetUrl(Guid teamId, Guid eventId) => $"/api/v1/teams/{teamId}/events/{eventId}";

	[Theory]
	[InlineData(TeamRole.Owner)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Coordinator)]
	public async Task RemoveEvent_AsCoordinatorOrHigher_Should_RemoveEventAndEventResponsesFromDatabase(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, initiatorRole, members)
			.WithEventTypes(5)
			.Generate();
		var events = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(team.EventTypes[0].Id)
			.WithRandomEventResponses(team.Members)
			.Generate(10);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Events.AddRange(events);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		var targetEvent = F.PickRandom(events);

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id, targetEvent.Id), CancellationToken);

		//assert
		response.ShouldBe200OK();

		var rest = await UseDbContextAsync(dbContext =>
		{
			return dbContext.Events
				.Include(e => e.EventResponses.OrderBy(er => er.TimeStampUtc))
				.OrderBy(e => e.Id)
				.ToListAsync();
		});
		rest.ShouldHaveSameValuesAs(events.Except([targetEvent]).OrderBy(e => e.Id));
	}

	[Fact]
	public async Task RemoveEvent_AsMember_Should_ResultInForbidden()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, TeamRole.Member, members)
			.WithEventTypes(5)
			.Generate();
		var events = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(team.EventTypes[0].Id)
			.WithRandomEventResponses(team.Members)
			.Generate(10);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Events.AddRange(events);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		var targetEvent = F.PickRandom(events);

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id, targetEvent.Id), CancellationToken);

		//assert
		response.ShouldBe403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToDeleteEvents);
	}

	[Fact]
	public async Task RemoveEvent_ThatDoesNotExist_AsOwner_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();
		var events = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(team.EventTypes[0].Id)
			.WithRandomEventResponses(team.Members)
			.Generate(10);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Events.AddRange(events);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		var targetEventId = EventId.New();

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id, targetEventId), CancellationToken);

		//assert
		response.ShouldBe404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(EventErrors.EventNotFound);
	}

	[Fact]
	public async Task RemoveEvent_FromUnExistingTeam_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		var targetTeamId = TeamId.New();
		var targetEventId = EventId.New();

		//act
		var response = await Client.DeleteAsync(GetUrl(targetTeamId, targetEventId), CancellationToken);

		//assert
		response.ShouldBe404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task RemoveEvent_OfAnotherTeam_AsOwner_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);

		var team1 = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();
		var team1Events = EventGenerators.Event
			.ForTeam(team1.Id)
			.WithEventType(team1.EventTypes[0].Id)
			.WithRandomEventResponses(team1.Members)
			.Generate(10);

		var team2 = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();
		var team2Events = EventGenerators.Event
			.ForTeam(team2.Id)
			.WithEventType(team2.EventTypes[0].Id)
			.WithRandomEventResponses(team2.Members)
			.Generate(10);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.AddRange([team1, team2]);
			dbContext.Events.AddRange(team1Events);
			dbContext.Events.AddRange(team2Events);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		var targetTeamId = team1.Id;
		var targetEvent = F.PickRandom(team2Events);

		//act
		var response = await Client.DeleteAsync(GetUrl(targetTeamId, targetEvent.Id), CancellationToken);

		//assert
		response.ShouldBe404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(EventErrors.EventNotFound);
	}
}
