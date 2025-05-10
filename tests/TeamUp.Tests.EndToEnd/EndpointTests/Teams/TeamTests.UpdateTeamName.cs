namespace TeamUp.Tests.EndToEnd.EndpointTests.Teams;

public sealed class UpdateTeamNameTests(AppFixture app) : TeamTests(app)
{
	public static string GetUrl(TeamId teamId) => GetUrl(teamId.Value);
	public static string GetUrl(Guid teamId) => $"/api/v1/teams/{teamId}";

	[Fact]
	public async Task UpdateTeamName_AsOwner_Should_UpdateTeamNameInDatabase()
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

		var request = new UpdateTeamNameRequest
		{
			Name = TeamGenerators.GenerateValidTeamName()
		};

		//act
		var response = await Client.PatchAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.ShouldBe200OK();

		var updatedTeam = await UseDbContextAsync(dbContext => dbContext.Teams.FindAsync(team.Id));

		updatedTeam.ShouldNotBeNull();
		updatedTeam.Name.Should().Be(request.Name);
	}

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	public async Task UpdateTeamName_AsAdminOrLower_Should_ResultInForbidden(TeamRole teamRole)
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

		var request = new UpdateTeamNameRequest
		{
			Name = TeamGenerators.GenerateValidTeamName()
		};

		//act
		var response = await Client.PatchAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.ShouldBe403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToChangeTeamName);
	}

	[Fact]
	public async Task UpdateTeamName_OfUnExistingTeam_Should_ResultInForbidden()
	{
		//arrange
		var user = UserGenerators.User.Generate();
		var teamId = Guid.NewGuid();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		var request = new UpdateTeamNameRequest
		{
			Name = TeamGenerators.GenerateValidTeamName()
		};

		//act
		var response = await Client.PatchAsJsonAsync(GetUrl(teamId), request);

		//assert
		response.ShouldBe404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task UpdateTeamName_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = new UpdateTeamNameRequest
		{
			Name = TeamGenerators.GenerateValidTeamName()
		};

		//act
		var response = await Client.PatchAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.ShouldBe403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Theory]
	[InlineData("")]
	[InlineData("x")]
	[InlineData("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
	public async Task UpdateTeamName_WithInvalidTeamName_AsOwner_Should_ResultInBadRequest_ValidationErrors(string name)
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

		var request = new UpdateTeamNameRequest
		{
			Name = name
		};

		//act
		var response = await Client.PatchAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.ShouldBe400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor(nameof(UpdateTeamNameRequest.Name));
	}
}
