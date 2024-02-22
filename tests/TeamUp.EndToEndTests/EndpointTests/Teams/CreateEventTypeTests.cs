using TeamUp.Contracts.Teams;

namespace TeamUp.EndToEndTests.EndpointTests.Teams;

public sealed class CreateEventTypeTests : BaseTeamTests
{
	public CreateEventTypeTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	[Theory]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task CreateEventType_AsCoordinatorOrHigher_Should_AddNewEventTypeToTeamInDatabase(TeamRole teamRole)
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(initiatorUser, teamRole, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = TeamGenerator.ValidUpsertEventTypeRequest.Generate();

		//act
		var response = await Client.PostAsJsonAsync($"/api/v1/teams/{team.Id.Value}/event-types", request);

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
		var owner = UserGenerator.ActivatedUser.Generate();
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(18);
		var team = TeamGenerator.GenerateTeamWith(owner, members, (initiatorUser, TeamRole.Member));

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = TeamGenerator.ValidUpsertEventTypeRequest.Generate();

		//act
		var response = await Client.PostAsJsonAsync($"/api/v1/teams/{team.Id.Value}/event-types", request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToCreateEventTypes);
	}

	[Fact]
	public async Task CreateEventType_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(18);
		var team = TeamGenerator.GenerateTeamWith(owner, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = TeamGenerator.ValidUpsertEventTypeRequest.Generate();

		//act
		var response = await Client.PostAsJsonAsync($"/api/v1/teams/{team.Id.Value}/event-types", request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task CreateEventType_InUnExistingTeam_Should_ResultInNotFound()
	{
		//arrange
		var user = UserGenerator.ActivatedUser.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		var teamId = F.Random.Guid();
		var request = TeamGenerator.ValidUpsertEventTypeRequest.Generate();

		//act
		var response = await Client.PostAsJsonAsync($"/api/v1/teams/{teamId}/event-types", request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Theory]
	[ClassData(typeof(TeamGenerator.InvalidUpsertEventTypeRequest))]
	public async Task CreateEventType_WithInvalidParameters_AsOwner_Should_ResultInBadRequest_ValidationErrors(InvalidRequest<UpsertEventTypeRequest> request)
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(owner, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(owner);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(owner);

		//act
		var response = await Client.PostAsJsonAsync($"/api/v1/teams/{team.Id.Value}/event-types", request.Request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor(request.InvalidProperty);
	}
}
