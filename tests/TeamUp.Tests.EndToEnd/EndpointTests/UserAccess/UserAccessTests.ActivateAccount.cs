namespace TeamUp.Tests.EndToEnd.EndpointTests.UserAccess;

public sealed class ActivateAccountTests(AppFixture app) : UserAccessTests(app)
{
	public static string GetUrl(UserId userId) => GetUrl(userId.Value);
	public static string GetUrl(Guid userId) => $"/api/v1/users/{userId}/activate";

	[Fact]
	public async Task ActivateAccount_ThatIsNotActivated_Should_SetUserStatusAsActivatedInDatabase()
	{
		//arrange
		var user = UserGenerators.User
			.Clone()
			.WithStatus(UserStatus.NotActivated)
			.Generate();

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

	[Fact]
	public async Task ActivateAccount_ThatIsActivated_Should_ResultInBadRequest_DomainError()
	{
		//arrange
		var user = UserGenerators.User
			.Clone()
			.WithStatus(UserStatus.Activated)
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync();
		});

		//act
		var response = await Client.PostAsync(GetUrl(user.Id), null);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UserErrors.AccountAlreadyActivated);
	}

	[Fact]
	public async Task ActivateAccount_ThatIsGenerated_Should_ResultInBadRequest_DomainError()
	{
		//arrange
		var user = UserGenerators.User
			.Clone()
			.WithStatus(UserStatus.Generated)
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync();
		});

		//act
		var response = await Client.PostAsync(GetUrl(user.Id), null);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UserErrors.CannotActivateGeneratedAccount);
	}
}
