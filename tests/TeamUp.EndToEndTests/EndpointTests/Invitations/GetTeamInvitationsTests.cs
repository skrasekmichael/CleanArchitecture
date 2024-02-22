using TeamUp.Contracts.Invitations;
using TeamUp.Contracts.Teams;

namespace TeamUp.EndToEndTests.EndpointTests.Invitations;

public sealed class GetTeamInvitationsTests : BaseInvitationTests
{
	public GetTeamInvitationsTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	[Theory]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task GetTeamInvitations_AsCoordinatorOrHigher_Should_ReturnListOfInvitations(TeamRole teamRole)
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(initiatorUser, teamRole, members);

		//need to remove milliseconds as when saving to database, there is slight shift
		var utcNow = new DateTime(DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond, DateTimeKind.Utc);
		var invitations = InvitationGenerator.GenerateInvitations(team.Id, utcNow, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.AddRange(invitations);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.GetAsync($"/api/v1/invitations/teams/{team.Id.Value}");

		//assert
		response.Should().Be200Ok();

		var teamInvitations = await response.Content.ReadFromJsonAsync<List<TeamInvitationResponse>>();
		invitations.Should().BeEquivalentTo(teamInvitations, o => o.ExcludingMissingMembers());
	}

	[Fact]
	public async Task GetTeamInvitations_AsMember_Should_ResultInForbidden()
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(initiatorUser, TeamRole.Member, members);
		var invitations = InvitationGenerator.GenerateInvitations(team.Id, DateTime.UtcNow, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.AddRange(invitations);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.GetAsync($"/api/v1/invitations/teams/{team.Id.Value}");

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToReadInvitationList);
	}

	[Fact]
	public async Task GetTeamInvitations_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(owner, members);
		var invitations = InvitationGenerator.GenerateInvitations(team.Id, DateTime.UtcNow, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.AddRange(invitations);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.GetAsync($"/api/v1/invitations/teams/{team.Id.Value}");

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task GetTeamInvitations_OfUnExistingTeam_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var teamId = F.Random.Guid();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.GetAsync($"/api/v1/invitations/teams/{teamId}");

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}
}
