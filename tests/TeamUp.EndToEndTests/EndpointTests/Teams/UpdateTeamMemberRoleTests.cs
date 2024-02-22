using Microsoft.EntityFrameworkCore;

using TeamUp.Contracts.Teams;

namespace TeamUp.EndToEndTests.EndpointTests.Teams;

public sealed class UpdateTeamMemberRoleTests : BaseTeamTests
{
	public UpdateTeamMemberRoleTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	[Theory]
	[InlineData(TeamRole.Admin, TeamRole.Member, TeamRole.Admin)]
	[InlineData(TeamRole.Admin, TeamRole.Member, TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin, TeamRole.Coordinator, TeamRole.Admin)]
	[InlineData(TeamRole.Admin, TeamRole.Coordinator, TeamRole.Member)]
	[InlineData(TeamRole.Admin, TeamRole.Admin, TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin, TeamRole.Admin, TeamRole.Member)]
	[InlineData(TeamRole.Owner, TeamRole.Member, TeamRole.Admin)]
	[InlineData(TeamRole.Owner, TeamRole.Member, TeamRole.Coordinator)]
	[InlineData(TeamRole.Owner, TeamRole.Coordinator, TeamRole.Admin)]
	[InlineData(TeamRole.Owner, TeamRole.Coordinator, TeamRole.Member)]
	[InlineData(TeamRole.Owner, TeamRole.Admin, TeamRole.Coordinator)]
	[InlineData(TeamRole.Owner, TeamRole.Admin, TeamRole.Member)]
	public async Task UpdateTeamRole_OfAdminOrLower_AsOwnerOrAdmin_Should_UpdateTeamMemberRoleInDatabase(TeamRole initiatorRole, TeamRole targetRole, TeamRole newRole)
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var targetUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(18);
		var team = TeamGenerator.GenerateTeamWith(initiatorUser, initiatorRole, targetUser, targetRole, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var targetMemberId = team.Members.First(member => member.UserId == targetUser.Id).Id;
		var request = new UpdateTeamRoleRequest
		{
			Role = newRole
		};

		//act
		var response = await Client.PutAsJsonAsync($"/api/v1/teams/{team.Id.Value}/{targetMemberId.Value}/role", request);

		//assert
		response.Should().Be200Ok();

		var teamMembers = await UseDbContextAsync(dbContext =>
		{
			return dbContext
				.Set<TeamMember>()
				.Where(teamMember => teamMember.TeamId == team.Id)
				.ToListAsync();
		});

		teamMembers.Should().ContainSingle(member => member.Role == TeamRole.Owner);

		var updatedMember = teamMembers.SingleOrDefault(member => member.UserId == targetUser.Id);
		updatedMember.ShouldNotBeNull();
		updatedMember.Role.Should().Be(newRole);

		teamMembers.Except([updatedMember])
			.Should().OnlyContain(member => TeamContainsMemberWithSameRole(team, member));
	}

	[Theory]
	[InlineData(TeamRole.Coordinator, TeamRole.Member, TeamRole.Admin)]
	[InlineData(TeamRole.Coordinator, TeamRole.Member, TeamRole.Coordinator)]
	[InlineData(TeamRole.Coordinator, TeamRole.Coordinator, TeamRole.Admin)]
	[InlineData(TeamRole.Coordinator, TeamRole.Coordinator, TeamRole.Member)]
	[InlineData(TeamRole.Coordinator, TeamRole.Admin, TeamRole.Coordinator)]
	[InlineData(TeamRole.Coordinator, TeamRole.Admin, TeamRole.Member)]
	[InlineData(TeamRole.Member, TeamRole.Member, TeamRole.Admin)]
	[InlineData(TeamRole.Member, TeamRole.Member, TeamRole.Coordinator)]
	[InlineData(TeamRole.Member, TeamRole.Coordinator, TeamRole.Admin)]
	[InlineData(TeamRole.Member, TeamRole.Coordinator, TeamRole.Member)]
	[InlineData(TeamRole.Member, TeamRole.Admin, TeamRole.Coordinator)]
	[InlineData(TeamRole.Member, TeamRole.Admin, TeamRole.Member)]
	public async Task UpdateTeamRole_OfAdminOrLower_AsCoordinatorOrMember_Should_ResultInForbidden(TeamRole initiatorRole, TeamRole targetRole, TeamRole newRole)
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var targetUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(18);
		var team = TeamGenerator.GenerateTeamWith(initiatorUser, initiatorRole, targetUser, targetRole, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var targetMemberId = team.Members.First(member => member.UserId == targetUser.Id).Id;
		var request = new UpdateTeamRoleRequest
		{
			Role = newRole
		};

		//act
		var response = await Client.PutAsJsonAsync($"/api/v1/teams/{team.Id.Value}/{targetMemberId.Value}/role", request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToUpdateTeamRoles);
	}

	[Theory]
	[InlineData(TeamRole.Admin, TeamRole.Member)]
	[InlineData(TeamRole.Admin, TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin, TeamRole.Admin)]
	[InlineData(TeamRole.Owner, TeamRole.Member)]
	[InlineData(TeamRole.Owner, TeamRole.Coordinator)]
	[InlineData(TeamRole.Owner, TeamRole.Admin)]
	public async Task UpdateTeamRole_OfOwner_AsOwnerOrAdmin_Should_ResultInBadRequest_DomainError(TeamRole initiatorRole, TeamRole newRole)
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(initiatorUser, initiatorRole, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var targetMemberId = team.Members.First(member => member.Role == TeamRole.Owner).Id;
		var request = new UpdateTeamRoleRequest
		{
			Role = newRole
		};

		//act
		var response = await Client.PutAsJsonAsync($"/api/v1/teams/{team.Id.Value}/{targetMemberId.Value}/role", request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.CannotChangeTeamOwnersRole);
	}

	[Theory]
	[InlineData((TeamRole)4)]
	[InlineData((TeamRole)99)]
	[InlineData((TeamRole)(-1))]
	public async Task UpdateTeamRole_OfMember_ToInvalidValue_AsOwner_Should_ResultInBadRequest_ValidationErrors(TeamRole newRole)
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var targetUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(owner, members, (targetUser, TeamRole.Member));


		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(owner);

		var targetMemberId = team.Members.First(member => member.UserId == targetUser.Id).Id;
		var request = new UpdateTeamRoleRequest
		{
			Role = newRole
		};

		//act
		var response = await Client.PutAsJsonAsync($"/api/v1/teams/{team.Id.Value}/{targetMemberId.Value}/role", request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor(nameof(UpdateTeamRoleRequest.Role));
	}

	[Fact]
	public async Task UpdateTeamRole_OfUnExistingMember_AsOwner_Should_ResultInNotFound()
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

		var targetMemberId = F.Random.Guid();
		var request = new UpdateTeamRoleRequest
		{
			Role = TeamRole.Member
		};

		//act
		var response = await Client.PutAsJsonAsync($"/api/v1/teams/{team.Id.Value}/{targetMemberId}/role", request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.MemberNotFound);
	}

	[Fact]
	public async Task UpdateTeamRole_InUnExistingTeam_Should_ResultInNotFound()
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
		var targetMemberId = F.Random.Guid();
		var request = new UpdateTeamRoleRequest
		{
			Role = TeamRole.Member
		};

		//act
		var response = await Client.PutAsJsonAsync($"/api/v1/teams/{teamId}/{targetMemberId}/role", request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task UpdateTeamRole_WhenNotMemberOfTeam_Should_ResultInForbidden()
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

		var targetMemberId = team.Members.First(member => member.Role != TeamRole.Owner).Id;
		var request = new UpdateTeamRoleRequest
		{
			Role = TeamRole.Member
		};

		//act
		var response = await Client.PutAsJsonAsync($"/api/v1/teams/{team.Id.Value}/{targetMemberId.Value}/role", request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}
}
