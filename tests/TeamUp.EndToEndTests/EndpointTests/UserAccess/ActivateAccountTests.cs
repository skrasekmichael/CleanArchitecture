namespace TeamUp.EndToEndTests.EndpointTests.UserAccess;

public sealed class ActivateAccountTests : BaseUserAccessTests
{
	public ActivateAccountTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	[Fact]
	public async Task ActivateAccount_Should_SetUserStatusAsActivatedInDatabase()
	{
		//arrange
		var user = UserGenerator.NotActivatedUser.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync();
		});

		//act
		var response = await Client.PostAsJsonAsync($"/api/v1/users/{user.Id.Value}/activate", EmptyObject);

		//assert
		response.Should().Be200Ok();

		var activatedUser = await UseDbContextAsync(dbContext => dbContext.Users.FindAsync(user.Id));
		activatedUser.ShouldNotBeNull();
		activatedUser.Status.Should().Be(UserStatus.Activated);
	}
}
