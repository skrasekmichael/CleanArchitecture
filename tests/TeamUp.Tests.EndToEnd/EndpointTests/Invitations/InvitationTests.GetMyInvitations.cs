namespace TeamUp.Tests.EndToEnd.EndpointTests.Invitations;

public sealed class GetMyInvitationsTests(AppFixture app) : InvitationTests(app)
{
	public const string URL = "/api/v1/invitations";

	[Fact]
	public async Task GetMyInvitations_Should_ReturnListOfInvitations()
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var teams = TeamGenerators.Team.WithMembers(owner, members).Generate(3);
		var invitations = InvitationGenerators.GenerateUserInvitations(initiatorUser.Id, DateTime.UtcNow.DropMicroSeconds(), teams);

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
		response.ShouldBe200OK();

		var userInvitations = await response.ReadFromJsonAsync<List<InvitationResponse>>();
		userInvitations.ShouldNotBeNull();
		invitations.ShouldHaveSameValuesAs(userInvitations.OrderBy(i => i.Id));
	}

	[Fact]
	public async Task GetMyInvitations_WhenNotInvited_Should_ReturnEmptyList()
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var teams = TeamGenerators.Team.WithMembers(owner, members).Generate(3);

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
		response.ShouldBe200OK();

		var invitations = await response.ReadFromJsonAsync<List<InvitationResponse>>();
		invitations.ShouldBeEmpty();
	}
}
