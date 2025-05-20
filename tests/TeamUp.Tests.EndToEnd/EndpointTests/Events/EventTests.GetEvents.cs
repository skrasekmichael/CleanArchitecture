namespace TeamUp.Tests.EndToEnd.EndpointTests.Events;

public sealed class GetEventsTests(AppFixture app) : EventTests(app)
{
	public static string GetUrl(TeamId teamId, DateTime? from) => GetUrl(teamId.Value, from);
	public static string GetUrl(Guid teamId, DateTime? from) => from switch
	{
		null => $"/api/v1/teams/{teamId}/events",
		DateTime fromUtc => $"/api/v1/teams/{teamId}/events?fromUtc={fromUtc:o}"
	};

	[Theory]
	[InlineData(TeamRole.Owner)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Member)]
	public async Task GetEvents_InFuture_AsTeamMember_Should_ReturnListOfEvents(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, initiatorRole, members)
			.WithEventTypes(5)
			.Generate();
		var eventType = team.EventTypes[0];
		var events = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithRandomEventResponses(team.Members)
			.Generate(40);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Events.AddRange(events);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var expectedEvents = events
			.Where(e => e.ToUtc > DateTime.UtcNow)
			.OrderBy(e => e.Id)
			.ToList();

		//act
		var response = await Client.GetAsync(GetUrl(team.Id, null));

		//assert
		response.ShouldBe200OK();

		var returnedEvents = await response.ReadFromJsonAsync<List<EventSlimResponse>>();
		returnedEvents.ShouldNotBeNull();

		expectedEvents.ShouldHaveSameValuesAs(returnedEvents.OrderBy(e => e.Id));
		returnedEvents.ForEach(e => EventShouldContainCorrectReplyCount(e, expectedEvents));
	}

	[Fact]
	public async Task GetEvents_FromOldDate_AsOwner_Should_ReturnListOfAllEvents()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();
		var eventType = team.EventTypes[0];
		var events = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithRandomEventResponses(team.Members)
			.Generate(40);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Events.AddRange(events);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var oldestEventDate = events.Select(e => e.FromUtc).Min();

		//act
		var response = await Client.GetAsync(GetUrl(team.Id, oldestEventDate));

		//assert
		response.ShouldBe200OK();

		var returnedEvents = await response.ReadFromJsonAsync<List<EventSlimResponse>>();
		returnedEvents.ShouldNotBeNull();

		events.OrderBy(e => e.Id).ShouldHaveSameValuesAs(returnedEvents.OrderBy(e => e.Id));
		returnedEvents.ForEach(e => EventShouldContainCorrectReplyCount(e, events));
	}

	[Fact]
	public async Task GetEvents_FromFutureDate_AsOwner_Should_ReturnEmptyList()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();
		var eventType = team.EventTypes[0];
		var events = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithRandomEventResponses(team.Members)
			.Generate(40);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Events.AddRange(events);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var futureDate = events.Select(e => e.ToUtc).Max().AddHours(1).AsUtc();

		//act
		var response = await Client.GetAsync(GetUrl(team.Id, futureDate));

		//assert
		response.ShouldBe200OK();

		var returnedEvents = await response.ReadFromJsonAsync<List<EventSlimResponse>>();
		returnedEvents.ShouldBeEmpty();
	}

	[Fact]
	public async Task GetEvents_FromUnExistingTeam_AsOwner_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var teamId = Guid.NewGuid();

		//act
		var response = await Client.GetAsync(GetUrl(teamId, null));

		//assert
		response.ShouldBe404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task GetEvents_WhenNotMemberOfTeam_AsOwner_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(owner, members)
			.WithEventTypes(5)
			.Generate();
		var eventType = team.EventTypes[0];
		var events = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithRandomEventResponses(team.Members)
			.Generate(40);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Events.AddRange(events);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.GetAsync(GetUrl(team.Id, null));

		//assert
		response.ShouldBe403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}
}
