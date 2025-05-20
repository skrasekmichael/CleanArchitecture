namespace TeamUp.Tests.EndToEnd.EndpointTests.Teams;

public sealed class RemoveTeamMemberTests(AppFixture app) : TeamTests(app)
{
	public static string GetUrl(TeamId teamId, TeamMemberId memberId) => GetUrl(teamId.Value, memberId.Value);
	public static string GetUrl(Guid teamId, Guid memberId) => $"/api/v1/teams/{teamId}/members/{memberId}";

	[Theory]
	[InlineData(TeamRole.Admin, TeamRole.Member)]
	[InlineData(TeamRole.Admin, TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin, TeamRole.Admin)]
	[InlineData(TeamRole.Owner, TeamRole.Member)]
	[InlineData(TeamRole.Owner, TeamRole.Coordinator)]
	[InlineData(TeamRole.Owner, TeamRole.Admin)]
	public async Task RemoveTeamMember_AsOwnerOrAdmin_WhenRemovingAdminOrLower_Should_RemoveTeamMemberFromTeamInDatabase(TeamRole initiatorTeamRole, TeamRole targetMemberRole)
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var targetUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(18);
		var team = TeamGenerators.Team.WithMembers(initiatorUser, initiatorTeamRole, targetUser, targetMemberRole, members).Generate();

		var targetMemberId = team.Members.First(member => member.UserId == targetUser.Id).Id;

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id, targetMemberId));

		//assert
		response.ShouldBe200OK();

		await UseDbContextAsync(async dbContext =>
		{
			var targetMember = await dbContext.Set<TeamMember>().FindAsync(targetMemberId);
			targetMember.ShouldBeNull("this member has been removed");

			var targetMemberUser = await dbContext.Users.FindAsync(targetUser.Id);
			targetMemberUser.ShouldNotBeNull();
			targetMemberUser.ShouldHaveSameValuesAs(targetUser);
		});
	}

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	public async Task RemoveTeamMember_AsAdminOrLower_WhenRemovingMyself_Should_RemoveTeamMemberFromTeamInDatabase(TeamRole teamRole)
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(initiatorUser, teamRole, members).Generate();

		var targetMemberId = team.Members.First(member => member.UserId == initiatorUser.Id).Id;

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id, targetMemberId));

		//assert
		response.ShouldBe200OK();

		await UseDbContextAsync(async dbContext =>
		{
			var targetMember = await dbContext.Set<TeamMember>().FindAsync(targetMemberId);
			targetMember.ShouldBeNull("this member has been removed");

			var targetMemberUser = await dbContext.Users.FindAsync(initiatorUser.Id);
			targetMemberUser.ShouldNotBeNull();
			targetMemberUser.ShouldHaveSameValuesAs(initiatorUser);
		});
	}

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	public async Task RemoveTeamMember_AsCoordinatorOrMember_WhenRemovingTeamMember_Should_ResultInForbidden(TeamRole memberRole)
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var initiatorUser = UserGenerators.User.Generate();
		var targetUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(18);
		var team = TeamGenerators.Team.WithMembers(owner, members, (initiatorUser, memberRole), (targetUser, TeamRole.Member)).Generate();

		var targetMemberId = team.Members.First(member => member.UserId == targetUser.Id).Id;

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id, targetMemberId));

		//assert
		response.ShouldBe403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToRemoveTeamMembers);
	}

	[Fact]
	public async Task RemoveTeamMember_AsOwner_WhenRemovingMyself_Should_ResultInBadRequest_DomainError()
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();

		var targetMemberId = team.Members.First(member => member.UserId == owner.Id).Id;

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(owner);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(owner);

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id, targetMemberId));

		//assert
		response.ShouldBe400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.CannotRemoveTeamOwner);
	}

	[Fact]
	public async Task RemoveTeamMember_AsAdmin_WhenRemovingTeamOwner_Should_ResultInBadRequest_DomainError()
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var user = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(18);
		var team = TeamGenerators.Team.WithMembers(owner, members, (user, TeamRole.Admin)).Generate();

		var targetMemberId = team.Members.First(member => member.UserId == owner.Id).Id;

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, user]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(owner);

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id, targetMemberId));

		//assert
		response.ShouldBe400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.CannotRemoveTeamOwner);
	}

	[Fact]
	public async Task RemoveTeamMember_ThatDoesNotExist_AsOwner_Should_ResultInNotFound()
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();

		var targetMemberId = Guid.NewGuid();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(owner);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(owner);

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id.Value, targetMemberId));

		//assert
		response.ShouldBe404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.MemberNotFound);
	}

	[Fact]
	public async Task RemoveTeamMember_FromUnExistingTeam_Should_ResultInNotFound()
	{
		//arrange
		var user = UserGenerators.User.Generate();

		var teamId = Guid.NewGuid();
		var targetMemberId = Guid.NewGuid();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});
		Authenticate(user);

		//act
		var response = await Client.DeleteAsync(GetUrl(teamId, targetMemberId));

		//assert
		response.ShouldBe404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task RemoveTeamMember_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();

		var targetMemberId = team.Members.First(member => member.Role != TeamRole.Owner).Id;

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id, targetMemberId));

		//assert
		response.ShouldBe403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}
}
