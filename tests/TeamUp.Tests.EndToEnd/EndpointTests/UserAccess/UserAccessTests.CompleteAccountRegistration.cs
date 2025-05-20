namespace TeamUp.Tests.EndToEnd.EndpointTests.UserAccess;

public sealed class CompleteAccountRegistrationTests(AppFixture app) : UserAccessTests(app)
{
	public static string GetUrl(UserId userId) => GetUrl(userId.Value);
	public static string GetUrl(Guid userId) => $"/api/v1/users/{userId}/generated/complete";

	[Theory]
	[InlineData(UserStatus.Activated)]
	[InlineData(UserStatus.NotActivated)]
	public async Task CompleteAccountRegistration_ThatIsNotGenerated_Should_ResultInBadRequest_DomainError(UserStatus status)
	{
		//arrange
		var user = UserGenerators.User
			.Clone()
			.WithStatus(status)
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync();
		});

		//act
		Client.DefaultRequestHeaders.Add(UserConstants.HTTP_HEADER_PASSWORD, "password");
		var response = await Client.PostAsync(GetUrl(user.Id), null);

		//assert
		response.ShouldBe400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UserErrors.CannotCompleteRegistrationOfNonGeneratedAccount);
	}

	[Fact]
	public async Task CompleteAccountRegistration_ThatIsGenerated_Should_ResultInSettingPasswordAndUpdatingUserStatusInDatabase()
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
		Client.DefaultRequestHeaders.Add(UserConstants.HTTP_HEADER_PASSWORD, "password");
		var response = await Client.PostAsync(GetUrl(user.Id), null);

		//assert
		response.ShouldBe200OK();

		var activatedUser = await UseDbContextAsync(dbContext => dbContext.Users.FindAsync(user.Id));
		activatedUser.ShouldNotBeNull();
		activatedUser.Status.ShouldBe(UserStatus.Activated);
	}
}
