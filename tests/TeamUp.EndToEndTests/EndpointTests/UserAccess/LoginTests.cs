using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using TeamUp.Application.Users;
using TeamUp.Contracts.Users;

namespace TeamUp.EndToEndTests.EndpointTests.UserAccess;

public sealed class LoginTests : BaseUserAccessTests
{
	public LoginTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	[Fact]
	public async Task Login_AsActivatedUser_Should_GenerateValidJwtToken()
	{
		//arrange
		var passwordService = AppFactory.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerator.GenerateValidPassword();
		var user = UserGenerator.GenerateUser(passwordService.HashPassword(rawPassword), UserStatus.Activated);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync();
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

		var token = await response.ReadFromJsonAsync<string>();
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
	public async Task Login_AsInactivatedUser_Should_ResultInUnauthorized()
	{
		//arrange
		var passwordService = AppFactory.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerator.GenerateValidPassword();
		var user = UserGenerator.GenerateUser(passwordService.HashPassword(rawPassword), UserStatus.NotActivated);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync();
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

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(AuthenticationErrors.NotActivatedAccount);
	}

	[Fact]
	public async Task Login_AsUnExistingUser_Should_ResultInUnauthorized()
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

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(AuthenticationErrors.InvalidCredentials);
	}

	[Fact]
	public async Task Login_WithIncorrectPassword_Should_ResultInUnauthorized()
	{
		//arrange
		var passwordService = AppFactory.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerator.GenerateValidPassword();
		var user = UserGenerator.GenerateUser(passwordService.HashPassword(rawPassword), UserStatus.Activated);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync();
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

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(AuthenticationErrors.InvalidCredentials);
	}

}
