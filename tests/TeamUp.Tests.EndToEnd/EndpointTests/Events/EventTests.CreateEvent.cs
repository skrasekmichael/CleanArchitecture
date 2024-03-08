namespace TeamUp.Tests.EndToEnd.EndpointTests.Events;

public sealed class CreateEventTests : EventTests
{
	public CreateEventTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	public static string GetUrl(TeamId teamId) => GetUrl(teamId.Value);
	public static string GetUrl(Guid teamId) => $"/api/v1/teams/{teamId}/events";

	[Theory]
	[InlineData(TeamRole.Owner)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Coordinator)]
	public async Task CreateEvent_AsCoordinatorOrHigher_Should_CreateNewEventInDatabase(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUser = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, initiatorRole, members)
			.WithEventTypes(5)
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = EventGenerators.ValidCreateEventRequest
			.WithEventType(team.EventTypes[0].Id)
			.WithTime(DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(4).AddHours(1))
			.Generate();

		//act
		var response = await Client.PostAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.Should().Be201Created();

		var eventId = await response.ReadFromJsonAsync<EventId>();
		eventId.ShouldNotBeNull();

		var createdEvent = await UseDbContextAsync(dbContext => dbContext.Events.FindAsync(eventId));
		createdEvent.ShouldNotBeNull();
		createdEvent.Status.Should().Be(EventStatus.Open);
		createdEvent.TeamId.Should().Be(team.Id);
		createdEvent.Should().BeEquivalentTo(request, o => o.ExcludingMissingMembers());
	}

	[Fact]
	public async Task CreateEvent_AsMember_Should_ResultInForbidden()
	{
		//arrange
		var initiatorUser = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, TeamRole.Member, members)
			.WithEventTypes(5)
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = EventGenerators.ValidCreateEventRequest
			.WithEventType(team.EventTypes[0].Id)
			.WithTime(DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(4).AddHours(1))
			.Generate();

		//act
		var response = await Client.PostAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToCreateEvents);
	}

	[Fact]
	public async Task CreateEvent_WhenNotMember_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerators.ActivatedUser.Generate();
		var initiatorUser = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(owner, members)
			.WithEventTypes(5)
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = EventGenerators.ValidCreateEventRequest
			.WithEventType(team.EventTypes[0].Id)
			.WithTime(DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(4).AddHours(1))
			.Generate();

		//act
		var response = await Client.PostAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task CreateEvent_ForUnExistingTeam_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUser = UserGenerators.ActivatedUser.Generate();
		var teamId = Guid.NewGuid();
		var eventTypeId = EventTypeId.New();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = EventGenerators.ValidCreateEventRequest
			.WithEventType(eventTypeId)
			.WithTime(DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(4).AddHours(1))
			.Generate();

		//act
		var response = await Client.PostAsJsonAsync(GetUrl(teamId), request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task CreateEvent_WithUnExistingEventType_Should_ResultInNotFound()
	{
		//arrange
		var owner = UserGenerators.ActivatedUser.Generate();
		var initiatorUser = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(owner, members)
			.WithEventTypes(5)
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = EventGenerators.ValidCreateEventRequest
			.WithEventType(EventTypeId.New())
			.WithTime(DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(4).AddHours(1))
			.Generate();

		//act
		var response = await Client.PostAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.EventTypeNotFound);
	}

	[Theory]
	[ClassData(typeof(EventGenerators.InvalidCreateEventRequest))]
	public async Task CreateEvent_WithInvalidParameters_AsOwner_Should_ResultInBadRequest_ValidationError(InvalidRequest<CreateEventRequest> invalidRequest)
	{
		//arrange
		var initiatorUser = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = new CreateEventRequest
		{
			EventTypeId = team.EventTypes[0].Id,
			Description = invalidRequest.Request.Description,
			MeetTime = invalidRequest.Request.MeetTime,
			ReplyClosingTimeBeforeMeetTime = invalidRequest.Request.ReplyClosingTimeBeforeMeetTime,
			FromUtc = invalidRequest.Request.FromUtc,
			ToUtc = invalidRequest.Request.ToUtc,
		};

		//act
		var response = await Client.PostAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor(invalidRequest.InvalidProperty);
	}
}
