using TeamUp.Contracts.Users;

namespace TeamUp.EndToEndTests.EndpointTests.UserAccess;

public sealed class GetMyProfileTests : BaseUserAccessTests
{
	public GetMyProfileTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	[Fact]
	public async Task GetMyProfile_WhenUnauthenticated_Should_ResultInUnauthorized()
	{
		//arrange
		//act
		var response = await Client.GetAsync("/api/v1/users/my-profile");

		//assert
		response.Should().Be401Unauthorized();
	}

	[Fact]
	public async Task GetMyProfile_AsExistingUser_Should_ReturnUserDetails()
	{
		//arrange
		var user = UserGenerator.ActivatedUser.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		//act
		var response = await Client.GetAsync("/api/v1/users/my-profile");

		//assert
		response.Should().Be200Ok();

		var userResponse = await response.Content.ReadFromJsonAsync<UserResponse>(JsonSerializerOptions);
		user.Should().BeEquivalentTo(userResponse, o => o.ExcludingMissingMembers());
	}
}
