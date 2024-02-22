using TeamUp.Contracts.Teams;

namespace TeamUp.EndToEndTests.EndpointTests.Teams;

public sealed class UpdateTeamNameTests : BaseTeamTests
{
	public UpdateTeamNameTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	[Fact]
	public async Task UpdateTeamName_AsOwner_Should_UpdateTeamNameInDatabase()
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

		var request = new UpdateTeamNameRequest
		{
			Name = TeamGenerator.GenerateValidTeamName()
		};

		//act
		var response = await Client.PatchAsJsonAsync($"/api/v1/teams/{team.Id.Value}", request);

		//assert
		response.Should().Be200Ok();

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

		var request = new UpdateTeamNameRequest
		{
			Name = TeamGenerator.GenerateValidTeamName()
		};

		//act
		var response = await Client.PatchAsJsonAsync($"/api/v1/teams/{team.Id.Value}", request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToChangeTeamName);
	}

	[Fact]
	public async Task UpdateTeamName_OfUnExistingTeam_Should_ResultInForbidden()
	{
		//arrange
		var user = UserGenerator.ActivatedUser.Generate();
		var teamId = F.Random.Guid();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		var request = new UpdateTeamNameRequest
		{
			Name = TeamGenerator.GenerateValidTeamName()
		};

		//act
		var response = await Client.PatchAsJsonAsync($"/api/v1/teams/{teamId}", request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task UpdateTeamName_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(owner, members);

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
			Name = TeamGenerator.GenerateValidTeamName()
		};

		//act
		var response = await Client.PatchAsJsonAsync($"/api/v1/teams/{team.Id.Value}", request);

		//assert
		response.Should().Be403Forbidden();

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

		var request = new UpdateTeamNameRequest
		{
			Name = name
		};

		//act
		var response = await Client.PatchAsJsonAsync($"/api/v1/teams/{team.Id.Value}", request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor(nameof(UpdateTeamNameRequest.Name));
	}
}
