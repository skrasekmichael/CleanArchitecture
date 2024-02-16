namespace TeamUp.EndToEndTests.EndpointTests.Teams;

public sealed class RemoveTeamMemberTests : BaseTeamTests
{
	public RemoveTeamMemberTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	public async Task RemoveTeamMember_AsOwner_WhenRemovingAdminOrLower_Should_RemoveTeamMemberFromTeam(TeamRole targetMemberRole)
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var targetUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(18);
		var team = TeamGenerator.GenerateTeamWith(owner, members, (targetUser, targetMemberRole));

		var targetMemberId = team.Members.FirstOrDefault(member => member.UserId == targetUser.Id)!.Id;

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(owner);
			dbContext.Users.Add(targetUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(owner);

		//act
		var response = await Client.DeleteAsync($"/api/v1/teams/{team.Id.Value}/{targetMemberId.Value}");

		//assert
		response.Should().Be200Ok();

		await UseDbContextAsync(async dbContext =>
		{
			var targetMember = await dbContext.Set<TeamMember>().FindAsync(targetMemberId);
			targetMember.Should().BeNull("this member has been removed");

			var targetMemberUser = await dbContext.Users.FindAsync(targetUser.Id);
			targetMemberUser.ShouldNotBeNull();
			targetMemberUser.Should().BeEquivalentTo(targetUser);
		});
	}

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	public async Task RemoveTeamMember_AsAdmin_WhenRemovingAdminOrLower_Should_RemoveTeamMemberFromTeam(TeamRole targetMemberRole)
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var targetUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(18);
		var team = TeamGenerator.GenerateTeamWith(owner, members, (initiatorUser, TeamRole.Admin), (targetUser, targetMemberRole));

		var targetMemberId = team.Members.FirstOrDefault(member => member.UserId == targetUser.Id)!.Id;

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.DeleteAsync($"/api/v1/teams/{team.Id.Value}/{targetMemberId.Value}");

		//assert
		response.Should().Be200Ok();

		await UseDbContextAsync(async dbContext =>
		{
			var targetMember = await dbContext.Set<TeamMember>().FindAsync(targetMemberId);
			targetMember.Should().BeNull("this member has been removed");

			var targetMemberUser = await dbContext.Users.FindAsync(targetUser.Id);
			targetMemberUser.ShouldNotBeNull();
			targetMemberUser.Should().BeEquivalentTo(targetUser);
		});
	}

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	public async Task RemoveTeamMember_AsAdminOrLower_WhenRemovingMyself_Should_RemoveTeamMemberFromTeam(TeamRole memberRole)
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var user = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(18);
		var team = TeamGenerator.GenerateTeamWith(owner, members, (user, memberRole));

		var targetMemberId = team.Members.FirstOrDefault(member => member.UserId == user.Id)!.Id;

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, user]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		//act
		var response = await Client.DeleteAsync($"/api/v1/teams/{team.Id.Value}/{targetMemberId.Value}");

		//assert
		response.Should().Be200Ok();

		await UseDbContextAsync(async dbContext =>
		{
			var targetMember = await dbContext.Set<TeamMember>().FindAsync(targetMemberId);
			targetMember.Should().BeNull("this member has been removed");

			var targetMemberUser = await dbContext.Users.FindAsync(user.Id);
			targetMemberUser.ShouldNotBeNull();
			targetMemberUser.Should().BeEquivalentTo(user);
		});
	}

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	public async Task RemoveTeamMember_AsCoordinatorOrMember_WhenRemovingTeamMember_Should_ResultInForbidden(TeamRole memberRole)
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var targetUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(18);
		var team = TeamGenerator.GenerateTeamWith(owner, members, (initiatorUser, memberRole), (targetUser, TeamRole.Member));

		var targetMemberId = team.Members.FirstOrDefault(member => member.UserId == targetUser.Id)!.Id;

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.DeleteAsync($"/api/v1/teams/{team.Id.Value}/{targetMemberId.Value}");

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToRemoveTeamMembers);
	}

	[Fact]
	public async Task RemoveTeamMember_AsOwner_WhenRemovingMyself_Should_ResultInDomainError_BadRequest()
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(owner, members);

		var targetMemberId = team.Members.FirstOrDefault(member => member.UserId == owner.Id)!.Id;

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(owner);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(owner);

		//act
		var response = await Client.DeleteAsync($"/api/v1/teams/{team.Id.Value}/{targetMemberId.Value}");

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.CannotRemoveTeamOwner);
	}

	[Fact]
	public async Task RemoveTeamMember_AsAdmin_WhenRemovingTeamOwner_Should_ResultInDomainError_BadRequest()
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var user = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(18);
		var team = TeamGenerator.GenerateTeamWith(owner, members, (user, TeamRole.Admin));

		var targetMemberId = team.Members.FirstOrDefault(member => member.UserId == owner.Id)!.Id;

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, user]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(owner);

		//act
		var response = await Client.DeleteAsync($"/api/v1/teams/{team.Id.Value}/{targetMemberId.Value}");

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.CannotRemoveTeamOwner);
	}

	[Fact]
	public async Task RemoveTeamMember_ThatDoesNotExist_AsOwner_Should_ResultInNotFound()
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(owner, members);

		var targetMemberId = F.Random.Guid();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(owner);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(owner);

		//act
		var response = await Client.DeleteAsync($"/api/v1/teams/{team.Id.Value}/{targetMemberId}");

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.MemberNotFound);
	}

	[Fact]
	public async Task RemoveTeamMember_FromTeamThatDoesNotExist_Should_ResultInNotFound()
	{
		//arrange
		var user = UserGenerator.ActivatedUser.Generate();

		var teamId = F.Random.Guid();
		var targetMemberId = F.Random.Guid();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		//act
		var response = await Client.DeleteAsync($"/api/v1/teams/{teamId}/{targetMemberId}");

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task RemoveTeamMember_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(owner, members);

		var targetMemberId = team.Members.FirstOrDefault(member => member.Role != TeamRole.Owner)!.Id;

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.DeleteAsync($"/api/v1/teams/{team.Id.Value}/{targetMemberId.Value}");

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}
}
