using Microsoft.EntityFrameworkCore;
using TeamUp.Infrastructure.Core;

namespace TeamUp.Tests.EndToEnd.EndpointTests.UserAccess;

public sealed class RegisterUserTests(AppFixture app) : UserAccessTests(app)
{
	public const string URL = "/api/v1/users/register";

	[Fact]
	public async Task RegisterUser_Should_CreateNewUserInDatabase_And_SendActivationEmail()
	{
		//arrange
		var request = UserGenerators.ValidRegisterUserRequest.Generate();

		//act
		var response = await Client.PostAsJsonAsync(URL, request);

		//assert
		response.ShouldBe201Created();

		var userId = await response.ReadFromJsonAsync<UserId>();
		userId.ShouldNotBeNull();

		var user = await UseDbContextAsync(dbContext => dbContext.Users.FindAsync(userId));
		user.ShouldNotBeNull();

		user.Name.ShouldBe(request.Name);
		user.Email.ShouldBe(request.Email);

		await WaitForIntegrationEventsAsync(); //wait for email
		Inbox.ShouldContain(email => email.EmailAddress == request.Email);
	}

	[Fact]
	public async Task RegisterUser_WithAlreadyUsedEmail_Should_ResultInConflict()
	{
		//arrange
		var user = UserGenerators.User.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		var request = new RegisterUserRequest()
		{
			Email = user.Email,
			Name = F.Internet.UserName(),
			Password = UserGenerators.GenerateValidPassword(),
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request);

		//assert
		response.ShouldBe409Conflict();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UserErrors.ConflictingEmail);
	}

	[Theory]
	[ClassData(typeof(UserGenerators.InvalidRegisterUserRequests))]
	public async Task RegisterUser_WithInvalidProperties_Should_ResultInValidationErrors_BadRequest(InvalidRequest<RegisterUserRequest> request)
	{
		//arrange
		//act
		var response = await Client.PostAsJsonAsync(URL, request.Request);

		//assert
		response.ShouldBe400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor(request.InvalidProperty);
	}

	[Fact]
	public async Task RegisterUser_WhenConcurrentRegistrationWithSameEmailCompletes_Should_ResultInConflict()
	{
		//arrange
		var request = UserGenerators.ValidRegisterUserRequest.Generate();

		//act
		var (responseA, responseB) = await RunConcurrentRequestsAsync(
			() => Client.PostAsJsonAsync(URL, request),
			() => Client.PostAsJsonAsync(URL, request)
		);

		//assert
		responseA.ShouldBe201Created();
		responseB.ShouldBe409Conflict();

		var userId = await responseA.ReadFromJsonAsync<UserId>();
		userId.ShouldNotBeNull();

		var user = await UseDbContextAsync(dbContext => dbContext.Users.FindAsync(userId));
		user.ShouldNotBeNull();

		user.Name.ShouldBe(request.Name);
		user.Email.ShouldBe(request.Email);

		var singleUser = await UseDbContextAsync(dbContext => dbContext.Users.SingleAsync(user => user.Email == request.Email));
		user.ShouldHaveSameValuesAs(singleUser);

		var problemDetails = await responseB.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UnitOfWork.UniqueConstraintError);
	}
}
