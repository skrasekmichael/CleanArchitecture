using TeamUp.Contracts.Users;

namespace TeamUp.EndToEndTests.EndpointTests.UserAccess;

public sealed class ActivateAccountTests : BaseUserAccessTests
{
	public ActivateAccountTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

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
		var response = await Client.PostAsJsonAsync(GetUrl(user.Id), EmptyObject);

		//assert
		response.Should().Be200Ok();

		var activatedUser = await UseDbContextAsync(dbContext => dbContext.Users.FindAsync(user.Id));
		activatedUser.ShouldNotBeNull();
		activatedUser.Status.Should().Be(UserStatus.Activated);
	}
}
