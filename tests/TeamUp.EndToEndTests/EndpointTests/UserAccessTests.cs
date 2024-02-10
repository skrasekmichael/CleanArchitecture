using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using TeamUp.Application.Users;
using TeamUp.Contracts.Users;
using TeamUp.EndToEndTests.Extensions;

namespace TeamUp.EndToEndTests.EndpointTests;

public sealed class UserAccessTests : BaseEndpointTests
{
	public UserAccessTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

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

		await Task.Delay(6_000); //wait for email
		Inbox.Should().Contain(email => email.EmailAddress == request.Email);
	}

	[Fact]
	public async Task RegisterUser_WithAlreadyUsedEmail_Should_ReturnConflict()
	{
		//arrange
		var user = UserGenerator.ActivatedUser.Generate();

		await UseDbContextAsync(async dbContext =>
		{
			dbContext.Users.Add(user);
			await dbContext.SaveChangesAsync();
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
		problemDetails.ShouldNotBeNull();
	}

	[Theory]
	[ClassData(typeof(UserGenerator.InvalidRegisterUserRequests))]
	public async Task RegisterUser_WithInvalidProperties_Should_ReturnValidationErrors(RegisterUserRequest request)
	{
		//arrange

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/users/register", request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(JsonSerializerOptions);
		problemDetails.ShouldNotBeNull();
	}

	[Fact]
	public async Task Login_AsActivatedUser_Should_GenerateValidJwtToken()
	{
		//arrange
		var passwordService = AppFactory.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerator.GenerateValidPassword();
		var user = User.Create(
			F.Internet.UserName(),
			F.Internet.Email(),
			passwordService.HashPassword(rawPassword));

		user.Activate();
		user.ClearDomainEvents();

		await UseDbContextAsync(async dbContext =>
		{
			dbContext.Add(user);
			await dbContext.SaveChangesAsync();
		});

		var request = new LoginRequest
		{
			Email = user.Email,
			Password = rawPassword
		};

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/users/login", request);

		//assert
		response.Should().Be200Ok();

		var token = await response.Content.ReadFromJsonAsync<string>(JsonSerializerOptions);
		token.Should().NotBeNullOrEmpty();

		var handler = new JwtSecurityTokenHandler();
		var jwt = handler.ReadJwtToken(token);

		jwt.ShouldNotBeNull();
		jwt.ValidTo.Ticks.Should().BeGreaterThan(DateTime.UtcNow.Ticks);
		jwt.Claims.Select(claim => (claim.Type, claim.Value))
			.Should()
			.Contain([
				(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
				(ClaimTypes.Name, user.Name),
				(ClaimTypes.Email, user.Email)
			]);
	}

	[Fact]
	public async Task Login_AsInactivatedUser_Should_ReturnUnauthorized()
	{
		//arrange
		var passwordService = AppFactory.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerator.GenerateValidPassword();
		var user = User.Create(
			F.Internet.UserName(),
			F.Internet.Email(),
			passwordService.HashPassword(rawPassword));

		await UseDbContextAsync(async dbContext =>
		{
			dbContext.Add(user);
			await dbContext.SaveChangesAsync();
		});

		var request = new LoginRequest
		{
			Email = user.Email,
			Password = rawPassword
		};

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/users/login", request);

		//assert
		response.Should().Be401Unauthorized();
	}

	[Fact]
	public async Task Login_AsNonexistentUser_Should_ReturnUnauthorized()
	{
		//arrange

		var request = new LoginRequest
		{
			Email = F.Internet.Email(),
			Password = UserGenerator.GenerateValidPassword()
		};

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/users/login", request);

		//assert
		response.Should().Be401Unauthorized();
	}

	[Fact]
	public async Task Login_WithIncorrectPassword_Should_ReturnUnauthorized()
	{
		//arrange
		var passwordService = AppFactory.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerator.GenerateValidPassword();
		var user = User.Create(
			F.Internet.UserName(),
			F.Internet.Email(),
			passwordService.HashPassword(rawPassword));

		user.Activate();
		user.ClearDomainEvents();

		await UseDbContextAsync(async dbContext =>
		{
			dbContext.Add(user);
			await dbContext.SaveChangesAsync();
		});

		var request = new LoginRequest
		{
			Email = user.Email,
			Password = rawPassword + "x"
		};

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/users/login", request);

		//assert
		response.Should().Be401Unauthorized();
	}

	[Fact]
	public async Task ActivateAccount_Should_SetUserStatusAsActivatedInDatabase()
	{
		//arrange
		var passwordService = AppFactory.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerator.GenerateValidPassword();
		var user = User.Create(
			F.Internet.UserName(),
			F.Internet.Email(),
			passwordService.HashPassword(rawPassword));

		await UseDbContextAsync(async dbContext =>
		{
			dbContext.Add(user);
			await dbContext.SaveChangesAsync();
		});

		//act
		var response = await Client.PostAsJsonAsync($"/api/v1/users/{user.Id.Value}/activate", EmptyObject);

		//assert
		response.Should().Be200Ok();

		var activatedUser = await UseDbContextAsync(async dbContext => await dbContext.Users.FindAsync(user.Id));
		activatedUser.ShouldNotBeNull();
		activatedUser.Status.Should().Be(UserStatus.Activated);
	}

	[Fact]
	public async Task GetMyProfile_WhenUnauthenticated_Should_ReturnUnauthorized()
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

		await UseDbContextAsync(async dbContext =>
		{
			dbContext.Users.Add(user);
			await dbContext.SaveChangesAsync();
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
