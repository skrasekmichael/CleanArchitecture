using Microsoft.EntityFrameworkCore;
using TeamUp.Infrastructure.Core;

namespace TeamUp.Tests.EndToEnd.EndpointTests.Teams;

public sealed class ChangeOwnershipTests(AppFixture app) : TeamTests(app)
{
	public static string GetUrl(TeamId teamId) => GetUrl(teamId.Value);
	public static string GetUrl(Guid teamId) => $"/api/v1/teams/{teamId}/owner";

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	public async Task ChangeOwnership_ToAdminOrLower_AsOwner_Should_ChangeTeamOwnerInDatabase(TeamRole teamRole)
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var targetUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(18);
		var team = TeamGenerators.Team.WithMembers(owner, members, (targetUser, teamRole)).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(owner);

		var targetMemberId = team.Members.First(member => member.UserId == targetUser.Id).Id;

		//assert
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), targetMemberId.Value, CancellationToken);

		//act
		response.ShouldBe200OK();

		var teamMembers = await UseDbContextAsync(dbContext =>
		{
			return dbContext
				.Set<TeamMember>()
				.Where(member => member.TeamId == team.Id)
				.ToListAsync();
		});

		var originalOwner = teamMembers.SingleOrDefault(member => member.UserId == owner.Id);
		originalOwner.ShouldNotBeNull();
		originalOwner.Role.ShouldBe(TeamRole.Admin);

		var newOwner = teamMembers.SingleOrDefault(member => member.UserId == targetUser.Id);
		newOwner.ShouldNotBeNull();
		newOwner.Role.ShouldBe(TeamRole.Owner);

		teamMembers.Except([originalOwner, newOwner])
			.ShouldContain(member => TeamContainsMemberWithSameRole(team, member));
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
		var owner = UserGenerators.User.Generate();
		var initiatorUser = UserGenerators.User.Generate();
		var targetUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(17);
		var team = TeamGenerators.Team.WithMembers(owner, members, (targetUser, targetTeamRole), (initiatorUser, initiatorTeamRole)).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		var targetMemberId = team.Members.First(member => member.UserId == initiatorUser.Id).Id;

		//assert
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), targetMemberId.Value, CancellationToken);

		//act
		response.ShouldBe403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToChangeTeamOwnership);
	}

	[Fact]
	public async Task ChangeOwnership_ToUnExistingMember_AsOwner_Should_ResultInNotFound()
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
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(owner);

		var targetMemberId = Guid.NewGuid();

		//assert
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), targetMemberId, CancellationToken);

		//act
		response.ShouldBe404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.MemberNotFound);
	}

	[Fact]
	public async Task ChangeOwnership_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var initiatorUser = UserGenerators.User.Generate();
		var targetUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(18);
		var team = TeamGenerators.Team.WithMembers(owner, members, (targetUser, TeamRole.Member)).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		var targetMemberId = team.Members.First(member => member.UserId == targetUser.Id).Id.Value;

		//assert
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), targetMemberId, CancellationToken);

		//act
		response.ShouldBe403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task ChangeOwnership_OfUnExistingTeam_Should_ResultInNotFound()
	{
		//arrange
		var user = UserGenerators.User.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(user);

		var teamId = Guid.NewGuid();
		var targetMemberId = Guid.NewGuid();

		//assert
		var response = await Client.PutAsJsonAsync(GetUrl(teamId), targetMemberId, CancellationToken);

		//act
		response.ShouldBe404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task ChangeOwnership_AsOwner_WhenConcurrentUpdateCompletes_Should_ResultInConflict()
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var targetUserA = UserGenerators.User.Generate();
		var targetUserB = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(17);
		var team = TeamGenerators.Team
			.WithMembers(owner, members, (targetUserA, TeamRole.Member), (targetUserB, TeamRole.Member))
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, targetUserA, targetUserB]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(owner);

		var targetMemberAId = team.Members.First(member => member.UserId == targetUserA.Id).Id;
		var targetMemberBId = team.Members.First(member => member.UserId == targetUserB.Id).Id;

		//assert
		var (responseA, responseB) = await RunConcurrentRequestsAsync(
			() => Client.PutAsJsonAsync(GetUrl(team.Id), targetMemberAId.Value, CancellationToken),
			() => Client.PutAsJsonAsync(GetUrl(team.Id), targetMemberBId.Value, CancellationToken)
		);

		//act
		responseA.ShouldBe200OK();
		responseB.ShouldBe409Conflict();

		var teamMembers = await UseDbContextAsync(dbContext =>
		{
			return dbContext
				.Set<TeamMember>()
				.Where(member => member.TeamId == team.Id)
				.ToListAsync();
		});

		var originalOwner = teamMembers.Single(member => member.UserId == owner.Id);
		originalOwner.Role.ShouldBe(TeamRole.Admin);

		var newOwner = teamMembers.Single(member => member.UserId == targetUserA.Id);
		newOwner.Role.ShouldBe(TeamRole.Owner);

		var concurrentTarget = teamMembers.Single(member => member.UserId == targetUserB.Id);
		concurrentTarget.Role.ShouldBe(TeamRole.Member);

		teamMembers.Except([originalOwner, newOwner])
			.ShouldContain(member => TeamContainsMemberWithSameRole(team, member));

		var problemDetails = await responseB.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UnitOfWork.ConcurrencyError);
	}
}
