namespace TeamUp.Tests.EndToEnd.EndpointTests.Teams;

public sealed class CreateEventTypeTests(AppFixture app) : TeamTests(app)
{
	public static string GetUrl(TeamId teamId) => GetUrl(teamId.Value);
	public static string GetUrl(Guid teamId) => $"/api/v1/teams/{teamId}/event-types";

	[Theory]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task CreateEventType_AsCoordinatorOrHigher_Should_AddNewEventTypeToTeamInDatabase(TeamRole teamRole)
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(initiatorUser, teamRole, members).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = TeamGenerators.ValidUpsertEventTypeRequest.Generate();

		//act
		var response = await Client.PostAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.Should().Be201Created();

		var eventTypeId = await response.ReadFromJsonAsync<EventTypeId>();
		eventTypeId.ShouldNotBeNull();

		await UseDbContextAsync(async dbContext =>
		{
			var createdEventType = await dbContext.Set<EventType>().FindAsync(eventTypeId);
			createdEventType.Should().BeEquivalentTo(request);

			var updatedTeam = await dbContext.Teams.FindAsync(team.Id);
			updatedTeam.ShouldNotBeNull();
			updatedTeam.EventTypes.Should().ContainSingle();
			updatedTeam.EventTypes[0].Should().BeEquivalentTo(createdEventType);
		});
	}

	[Fact]
	public async Task CreateEventType_AsMember_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(18);
		var team = TeamGenerators.Team.WithMembers(owner, members, (initiatorUser, TeamRole.Member)).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = TeamGenerators.ValidUpsertEventTypeRequest.Generate();

		//act
		var response = await Client.PostAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToCreateEventTypes);
	}

	[Fact]
	public async Task CreateEventType_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(18);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = TeamGenerators.ValidUpsertEventTypeRequest.Generate();

		//act
		var response = await Client.PostAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task CreateEventType_InUnExistingTeam_Should_ResultInNotFound()
	{
		//arrange
		var user = UserGenerators.User.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		var teamId = Guid.NewGuid();
		var request = TeamGenerators.ValidUpsertEventTypeRequest.Generate();

		//act
		var response = await Client.PostAsJsonAsync(GetUrl(teamId), request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Theory]
	[ClassData(typeof(TeamGenerators.InvalidUpsertEventTypeRequest))]
	public async Task CreateEventType_WithInvalidParameters_AsOwner_Should_ResultInBadRequest_ValidationErrors(InvalidRequest<UpsertEventTypeRequest> request)
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
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
		var response = await Client.PostAsJsonAsync(GetUrl(team.Id), request.Request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor(request.InvalidProperty);
	}
}
