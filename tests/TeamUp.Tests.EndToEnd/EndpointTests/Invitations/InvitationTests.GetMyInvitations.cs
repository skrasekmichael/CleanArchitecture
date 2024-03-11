namespace TeamUp.Tests.EndToEnd.EndpointTests.Invitations;

public sealed class GetMyInvitationsTests(AppFixture app) : InvitationTests(app)
{
	public const string URL = "/api/v1/invitations";

	[Fact]
	public async Task GetMyInvitations_Should_ReturnListOfInvitations()
	{
		//arrange
		var owner = UserGenerators.ActivatedUser.Generate();
		var initiatorUser = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(19);
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
		response.Should().Be200Ok();

		var userInvitations = await response.ReadFromJsonAsync<List<InvitationResponse>>();
		invitations.Should().BeEquivalentTo(userInvitations, o => o.ExcludingMissingMembers());
	}

	[Fact]
	public async Task GetMyInvitations_WhenNotInvited_Should_ReturnEmptyList()
	{
		//arrange
		var owner = UserGenerators.ActivatedUser.Generate();
		var initiatorUser = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(19);
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
		response.Should().Be200Ok();

		var invitations = await response.ReadFromJsonAsync<List<InvitationResponse>>();
		invitations.Should().BeEmpty();
	}
}
