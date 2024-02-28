using TeamUp.Contracts.Users;

namespace TeamUp.EndToEndTests.EndpointTests.UserAccess;

public sealed class GetMyAccountDetailsTests : BaseUserAccessTests
{
	public GetMyAccountDetailsTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	public const string URL = "/api/v1/users";

	[Fact]
	public async Task GetMyProfile_WhenUnauthenticated_Should_ResultInUnauthorized()
	{
		//arrange
		//act
		var response = await Client.GetAsync(URL);

		//assert
		response.Should().Be401Unauthorized();
	}

	[Fact]
	public async Task GetMyProfile_AsExistingUser_Should_ReturnUserDetails()
	{
		//arrange
		var user = UserGenerators.ActivatedUser.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		//act
		var response = await Client.GetAsync(URL);

		//assert
		response.Should().Be200Ok();

		var userResponse = await response.ReadFromJsonAsync<AccountResponse>();
		user.Should().BeEquivalentTo(userResponse, o => o.ExcludingMissingMembers());
	}
}
