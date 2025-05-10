namespace TeamUp.Tests.EndToEnd.EndpointTests.UserAccess;

public sealed class GetMyAccountDetailsTests(AppFixture app) : UserAccessTests(app)
{
	public const string URL = "/api/v1/users";

	[Fact]
	public async Task GetMyProfile_WhenUnauthenticated_Should_ResultInUnauthorized()
	{
		//arrange
		//act
		var response = await Client.GetAsync(URL);

		//assert
		response.ShouldBe401Unauthorized();
	}

	[Fact]
	public async Task GetMyProfile_AsExistingUser_Should_ReturnUserDetails()
	{
		//arrange
		var user = UserGenerators.User.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		//act
		var response = await Client.GetAsync(URL);

		//assert
		response.ShouldBe200OK();

		var userResponse = await response.ReadFromJsonAsync<AccountResponse>();
		user.Should().BeEquivalentTo(userResponse, o => o.ExcludingMissingMembers());
	}
}
