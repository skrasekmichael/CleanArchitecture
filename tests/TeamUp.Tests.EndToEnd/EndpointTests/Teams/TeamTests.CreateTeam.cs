﻿using Microsoft.EntityFrameworkCore;

using TeamUp.Infrastructure.Core;

namespace TeamUp.Tests.EndToEnd.EndpointTests.Teams;

public sealed class CreateTeamTests(AppFixture app) : TeamTests(app)
{
	public const string URL = "/api/v1/teams";

	[Fact]
	public async Task CreateTeam_WhenOwns4Teams_Should_CreateNewTeamInDatabase_WithOneTeamOwner()
	{
		//arrange
		var user = UserGenerators.User.Generate();
		var ownedTeams = TeamGenerators.Team
			.WithOneOwner(user)
			.Generate(TeamConstants.MAX_NUMBER_OF_OWNED_TEAMS - 1);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			dbContext.Teams.AddRange(ownedTeams);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		var createTeamRequest = new CreateTeamRequest
		{
			Name = TeamGenerators.GenerateValidTeamName()
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, createTeamRequest);

		//assert
		response.Should().Be201Created();

		var teamId = await response.ReadFromJsonAsync<TeamId>();
		teamId.ShouldNotBeNull();

		var team = await UseDbContextAsync(dbContext =>
		{
			return dbContext.Teams
				.AsSplitQuery()
				.Include(team => team.Members)
				.Include(team => team.EventTypes)
				.FirstOrDefaultAsync(team => team.Id == teamId);
		});

		team.ShouldNotBeNull();
		team.Name.Should().Be(createTeamRequest.Name);
		team.EventTypes.Should().BeEmpty();

		var tm = team.Members[0];
		tm.UserId.Should().Be(user.Id);
		tm.TeamId.Should().Be(teamId);
		tm.Nickname.Should().Be(user.Name);
		tm.Role.Should().Be(TeamRole.Owner);
	}

	[Theory]
	[InlineData("")]
	[InlineData("xx")]
	[InlineData("x123456789012345678901234567890")]
	public async Task CreateTeam_WithInvalidName_Should_ResultInBadRequest_ValidationError(string name)
	{
		//arrange
		var user = UserGenerators.User.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		var createTeamRequest = new CreateTeamRequest
		{
			Name = name
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, createTeamRequest);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor("Name");
	}


	[Fact]
	public async Task CreateTeam_AsUnExistingUser_Should_ResultInNotFound()
	{
		//arrange
		var user = UserGenerators.User.Generate();

		Authenticate(user);

		var createTeamRequest = new CreateTeamRequest
		{
			Name = TeamGenerators.GenerateValidTeamName()
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, createTeamRequest);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UserErrors.AccountNotFound);
	}

	[Fact]
	public async Task CreateTeam_WhenOwns5Teams_Should_ResultInBadRequest_DomainError()
	{
		//arrange
		var user = UserGenerators.User.Generate();
		var ownedTeams = TeamGenerators.Team
			.WithOneOwner(user)
			.Generate(TeamConstants.MAX_NUMBER_OF_OWNED_TEAMS);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			dbContext.Teams.AddRange(ownedTeams);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		var createTeamRequest = new CreateTeamRequest
		{
			Name = TeamGenerators.GenerateValidTeamName()
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, createTeamRequest);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.CannotOwnSoManyTeams);
	}

	[Fact]
	public async Task CreateTeam_WhenOwns4Teams_AndConcurrentCreateTeamCompletes_Should_ResultInConflict()
	{
		//arrange
		var user = UserGenerators.User.Generate();
		var ownedTeams = TeamGenerators.Team
			.WithOneOwner(user)
			.Generate(TeamConstants.MAX_NUMBER_OF_OWNED_TEAMS - 1);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			dbContext.Teams.AddRange(ownedTeams);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		var createTeamRequest = new CreateTeamRequest
		{
			Name = TeamGenerators.GenerateValidTeamName()
		};

		//act
		var (responseA, responseB) = await RunConcurrentRequestsAsync(
			() => Client.PostAsJsonAsync(URL, createTeamRequest),
			() => Client.PostAsJsonAsync(URL, createTeamRequest)
		);

		//assert
		responseA.Should().Be201Created();
		responseB.Should().Be409Conflict();

		var teamId = await responseA.ReadFromJsonAsync<TeamId>();
		teamId.ShouldNotBeNull();

		await UseDbContextAsync(async dbContext =>
		{
			var ownedTeamsCount = await dbContext
				.Set<TeamMember>()
				.Where(member => member.UserId == user.Id && member.Role == TeamRole.Owner)
				.CountAsync();

			ownedTeamsCount.Should().Be(TeamConstants.MAX_NUMBER_OF_OWNED_TEAMS);

			var team = await dbContext.Teams
				.AsSplitQuery()
				.Include(team => team.Members)
				.Include(team => team.EventTypes)
				.FirstOrDefaultAsync(team => team.Id == teamId);

			team.ShouldNotBeNull();
			team.Name.Should().Be(createTeamRequest.Name);
			team.EventTypes.Should().BeEmpty();

			var tm = team.Members[0];
			tm.UserId.Should().Be(user.Id);
			tm.TeamId.Should().Be(teamId);
			tm.Nickname.Should().Be(user.Name);
			tm.Role.Should().Be(TeamRole.Owner);
		});

		var problemDetails = await responseB.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UnitOfWork.ConcurrencyError);
	}
}
