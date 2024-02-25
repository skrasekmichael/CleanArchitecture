using TeamUp.Contracts.Invitations;

namespace TeamUp.EndToEndTests.EndpointTests.Invitations;

public sealed class GetMyInvitationsTests : BaseInvitationTests
{
	public GetMyInvitationsTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	public const string URL = "/api/v1/invitations";

	[Fact]
	public async Task GetMyInvitations_Should_ReturnListOfInvitations()
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var teams = new List<Team>
		{
			TeamGenerator.GenerateTeamWith(owner, members),
			TeamGenerator.GenerateTeamWith(owner, members),
			TeamGenerator.GenerateTeamWith(owner, members)
		};

		//need to remove milliseconds as there is slight shift when saving to database
		var utcNow = new DateTime(DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond, DateTimeKind.Utc);
		var invitations = InvitationGenerator.GenerateUserInvitations(initiatorUser.Id, utcNow, teams);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.AddRange(teams);
			dbContext.Invitations.AddRange(invitations);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.GetAsync(URL);

		//assert
		response.Should().Be200Ok();

		var userInvitations = await response.ReadFromJsonAsync<List<InvitationResponse>>();
		invitations.Should().BeEquivalentTo(userInvitations, o => o.ExcludingMissingMembers());
	}

	[Fact]
	public async Task GetMyInvitations_WhenNotInvited_Should_ReturnEmptyList()
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var teams = new List<Team>
		{
			TeamGenerator.GenerateTeamWith(owner, members),
			TeamGenerator.GenerateTeamWith(owner, members),
			TeamGenerator.GenerateTeamWith(owner, members)
		};

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.AddRange(teams);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.GetAsync(URL);

		//assert
		response.Should().Be200Ok();

		var invitations = await response.ReadFromJsonAsync<List<InvitationResponse>>();
		invitations.Should().BeEmpty();
	}
}
