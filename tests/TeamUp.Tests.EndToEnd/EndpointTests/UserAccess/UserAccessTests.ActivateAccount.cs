namespace TeamUp.Tests.EndToEnd.EndpointTests.UserAccess;

public sealed class ActivateAccountTests(AppFixture app) : UserAccessTests(app)
{
	public static string GetUrl(UserId userId) => GetUrl(userId.Value);
	public static string GetUrl(Guid userId) => $"/api/v1/users/{userId}/activate";

	[Fact]
	public async Task ActivateAccount_Should_SetUserStatusAsActivatedInDatabase()
	{
		//arrange
		var user = UserGenerators.NotActivatedUser.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync();
		});

		//act
		var response = await Client.PostAsync(GetUrl(user.Id), null);

		//assert
		response.Should().Be200Ok();

		var activatedUser = await UseDbContextAsync(dbContext => dbContext.Users.FindAsync(user.Id));
		activatedUser.ShouldNotBeNull();
		activatedUser.Status.Should().Be(UserStatus.Activated);
	}
}
