using TeamUp.Contracts.Users;

namespace TeamUp.EndToEndTests.EndpointTests.UserAccess;

public sealed class RegisterUserTests : BaseUserAccessTests
{
	public RegisterUserTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	[Fact]
	public async Task RegisterUser_Should_CreateNewUserInDatabase_And_SendActivationEmail()
	{
		//arrange
		var request = UserGenerator.ValidRegisterUserRequest.Generate();

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/users/register", request);

		//assert
		response.Should().Be201Created();

		var userId = await response.Content.ReadFromJsonAsync<UserId>(JsonSerializerOptions);
		userId.ShouldNotBeNull();

		var user = await UseDbContextAsync(dbContext => dbContext.Users.FindAsync(userId));
		user.ShouldNotBeNull();

		user.Name.Should().BeEquivalentTo(request.Name);
		user.Email.Should().BeEquivalentTo(request.Email);
		user.Password.Should().NotBeEquivalentTo(request.Password);

		await WaitForIntegrationEventsAsync(); //wait for email
		Inbox.Should().Contain(email => email.EmailAddress == request.Email);
	}

	[Fact]
	public async Task RegisterUser_WithAlreadyUsedEmail_Should_ResultInConflict()
	{
		//arrange
		var user = UserGenerator.ActivatedUser.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		var request = new RegisterUserRequest()
		{
			Email = user.Email,
			Name = F.Internet.UserName(),
			Password = UserGenerator.GenerateValidPassword(),
		};

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/users/register", request);

		//assert
		response.Should().Be409Conflict();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonSerializerOptions);
		problemDetails.ShouldContainError(UserErrors.ConflictingEmail);
	}

	[Theory]
	[ClassData(typeof(UserGenerator.InvalidRegisterUserRequests))]
	public async Task RegisterUser_WithInvalidProperties_Should_ResultInValidationErrors_BadRequest(InvalidRequest<RegisterUserRequest> request)
	{
		//arrange
		//act
		var response = await Client.PostAsJsonAsync("/api/v1/users/register", request.Request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(JsonSerializerOptions);
		problemDetails.ShouldContainValidationErrorFor(request.InvalidProperty);
	}

}
