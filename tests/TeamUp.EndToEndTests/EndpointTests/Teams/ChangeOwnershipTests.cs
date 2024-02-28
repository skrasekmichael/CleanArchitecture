using Microsoft.EntityFrameworkCore;

using TeamUp.Contracts.Teams;

namespace TeamUp.EndToEndTests.EndpointTests.Teams;

public sealed class ChangeOwnershipTests : BaseTeamTests
{
	public ChangeOwnershipTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	public static string GetUrl(TeamId teamId) => GetUrl(teamId.Value);
	public static string GetUrl(Guid teamId) => $"/api/v1/teams/{teamId}/owner";

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	public async Task ChangeOwnership_ToAdminOrLower_AsOwner_Should_ChangeTeamOwnerInDatabase(TeamRole teamRole)
	{
		//arrange
		var owner = UserGenerators.ActivatedUser.Generate();
		var targetUser = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(18);
		var team = TeamGenerators.Team.WithMembers(owner, members, (targetUser, teamRole)).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(owner);

		var targetMemberId = team.Members.First(member => member.UserId == targetUser.Id).Id;

		//assert
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), targetMemberId.Value);

		//act
		response.Should().Be200Ok();

		var teamMembers = await UseDbContextAsync(dbContext =>
		{
			return dbContext
				.Set<TeamMember>()
				.Where(member => member.TeamId == team.Id)
				.ToListAsync();
		});

		var originalOwner = teamMembers.SingleOrDefault(member => member.UserId == owner.Id);
		originalOwner.ShouldNotBeNull();
		originalOwner.Role.Should().Be(TeamRole.Admin);

		var newOwner = teamMembers.SingleOrDefault(member => member.UserId == targetUser.Id);
		newOwner.ShouldNotBeNull();
		newOwner.Role.Should().Be(TeamRole.Owner);

		teamMembers
			.Except<TeamMember>([originalOwner, newOwner])
			.Should()
			.Contain(member => TeamContainsMemberWithSameRole(team, member));
	}

	[Theory]
	[InlineData(TeamRole.Member, TeamRole.Member)]
	[InlineData(TeamRole.Member, TeamRole.Coordinator)]
	[InlineData(TeamRole.Member, TeamRole.Admin)]
	[InlineData(TeamRole.Coordinator, TeamRole.Member)]
	[InlineData(TeamRole.Coordinator, TeamRole.Coordinator)]
	[InlineData(TeamRole.Coordinator, TeamRole.Admin)]
	[InlineData(TeamRole.Admin, TeamRole.Member)]
	[InlineData(TeamRole.Admin, TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin, TeamRole.Admin)]
	public async Task ChangeOwnership_ToAdminOrLower_AsAdminOrLower_Should_ResultInForbidden(TeamRole initiatorTeamRole, TeamRole targetTeamRole)
	{
		//arrange
		var owner = UserGenerators.ActivatedUser.Generate();
		var initiatorUser = UserGenerators.ActivatedUser.Generate();
		var targetUser = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(17);
		var team = TeamGenerators.Team.WithMembers(owner, members, (targetUser, targetTeamRole), (initiatorUser, initiatorTeamRole)).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var targetMemberId = team.Members.First(member => member.UserId == initiatorUser.Id).Id;

		//assert
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), targetMemberId.Value);

		//act
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToChangeTeamOwnership);
	}

	[Fact]
	public async Task ChangeOwnership_ToUnExistingMember_AsOwner_Should_ResultInNotFound()
	{
		//arrange
		var owner = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(19);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(owner);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(owner);

		var targetMemberId = F.Random.Guid();

		//assert
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), targetMemberId);

		//act
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.MemberNotFound);
	}

	[Fact]
	public async Task ChangeOwnership_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerators.ActivatedUser.Generate();
		var initiatorUser = UserGenerators.ActivatedUser.Generate();
		var targetUser = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(18);
		var team = TeamGenerators.Team.WithMembers(owner, members, (targetUser, TeamRole.Member)).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var targetMemberId = team.Members.First(member => member.UserId == targetUser.Id).Id.Value;

		//assert
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), targetMemberId);

		//act
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task ChangeOwnership_OfUnExistingTeam_Should_ResultInNotFound()
	{
		//arrange
		var user = UserGenerators.ActivatedUser.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		var teamId = F.Random.Guid();
		var targetMemberId = F.Random.Guid();

		//assert
		var response = await Client.PutAsJsonAsync(GetUrl(teamId), targetMemberId);

		//act
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}
}
