﻿using Microsoft.EntityFrameworkCore;

using TeamUp.Contracts.Teams;

namespace TeamUp.EndToEndTests.EndpointTests.Teams;

public sealed class TeamTests : BaseEndpointTests
{
	public TeamTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	[Fact]
	public async Task CreateTeam_Should_CreateNewTeamInDatabase_WithOneTeamMember()
	{
		//arrange
		var user = UserGenerator.ActivatedUser.Generate();
		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		var createTeamRequest = new CreateTeamRequest
		{
			Name = TeamGenerator.GenerateValidTeamName()
		};

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/teams", createTeamRequest);

		//assert
		response.Should().Be201Created();

		var teamId = await response.Content.ReadFromJsonAsync<TeamId>();
		teamId.ShouldNotBeNull();

		var team = await UseDbContextAsync(dbContext =>
			dbContext.Teams
				.Include(team => team.Members)
				.Include(team => team.EventTypes)
				.FirstOrDefaultAsync(team => team.Id == teamId));

		team.ShouldNotBeNull();
		team.Name.Should().Be(createTeamRequest.Name);
		team.EventTypes.Should().BeEmpty();

		var tm = team.Members[0];
		tm.UserId.Should().Be(user.Id);
		tm.TeamId.Should().Be(teamId);
		tm.Nickname.Should().Be(user.Name);
		tm.Role.Should().Be(TeamRole.Owner);
	}

	[Fact]
	public async Task DeleteTeam_AsOwner_Should_DeleteTeamInDatabase()
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
		var response = await Client.DeleteAsync($"/api/v1/teams/{team.Id.Value}");

		//assert
		response.Should().Be200Ok();

		await UseDbContextAsync(async dbContext =>
		{
			var deletedTeam = await dbContext.Teams.FindAsync(team.Id);
			deletedTeam.Should().BeNull();

			var members = await dbContext.Set<TeamMember>().ToListAsync();
			members.Should().BeEmpty();
		});
	}

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	public async Task DeleteTeam_AsAdminOrLower_Should_ResultInForbidden(TeamRole teamRole)
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(18);
		var team = TeamGenerator.GenerateTeamWith(owner, members, (initiatorUser, teamRole));

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.DeleteAsync($"/api/v1/teams/{team.Id.Value}");

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToDeleteTeam);
	}

	[Fact]
	public async Task DeleteTeam_ThatDoesNotExist_Should_ResultInNotFound()
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

		//act
		var response = await Client.DeleteAsync($"/api/v1/teams/{teamId}");

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task DeleteTeam_WhenNotMemberOfTeam_Should_ResultInForbidden()
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

		//act
		var response = await Client.DeleteAsync($"/api/v1/teams/{team.Id.Value}");

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task GetTeam_ThatDoesNotExist_Should_ResultInNotFound()
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

		//act
		var response = await Client.GetAsync($"/api/v1/teams/{teamId}");

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task GetTeam_WithOneMember_AsTeamOwner_Should_ReturnTeamFromDatabase()
	{
		//arrange
		var user = UserGenerator.ActivatedUser.Generate();
		var team = TeamGenerator.GenerateTeamWithOwner(user);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		//act
		var response = await Client.GetAsync($"/api/v1/teams/{team.Id.Value}");

		//assert
		response.Should().Be200Ok();

		var teamResponse = await response.Content.ReadFromJsonAsync<TeamResponse>();
		teamResponse.ShouldNotBeNull();
		team.Should().BeEquivalentTo(teamResponse, options =>
		{
			return options
				.ExcludingMissingMembers()
				.IgnoringCyclicReferences();
		});
	}

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	public async Task GetTeam_With20Members_AsAdminOrLower_Should_ReturnTeamFromDatabase(TeamRole asRole)
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var user = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(18);
		var team = TeamGenerator.GenerateTeamWith(owner, members, (user, asRole));

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(owner);
			dbContext.Users.Add(user);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		//act
		var response = await Client.GetAsync($"/api/v1/teams/{team.Id.Value}");

		//assert
		response.Should().Be200Ok();

		var teamResponse = await response.Content.ReadFromJsonAsync<TeamResponse>();
		teamResponse.ShouldNotBeNull();
		team.Should().BeEquivalentTo(teamResponse, options =>
		{
			return options
				.ExcludingMissingMembers()
				.IgnoringCyclicReferences();
		});
	}

	[Fact]
	public async Task GetTeam_WhenNotMemberOfTeam_Should_ResultInForbidden()
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

		//act
		var response = await Client.GetAsync($"/api/v1/teams/{team.Id.Value}");

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

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
		var owner = UserGenerator.ActivatedUser.Generate();
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(owner, members, (initiatorUser, teamRole));

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

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToChangeTeamName);
	}

	[Fact]
	public async Task UpdateTeamName_ThatDoesNotExist_Should_ResultInForbidden()
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

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
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

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Theory]
	[InlineData("")]
	[InlineData("x")]
	[InlineData("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
	public async Task UpdateTeamName_WithInvalidTeamName_AsOwner_Should_ResultInBadRequest(string name)
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

		var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
		problemDetails.ShouldContainValidationErrorFor(nameof(UpdateTeamNameRequest.Name));
	}
}
