using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TeamUp.Application.Users;

namespace TeamUp.Tests.EndToEnd.EndpointTests.UserAccess;

public sealed class LoginTests(AppFixture app) : UserAccessTests(app)
{
	public const string URL = "/api/v1/users/login";

	[Fact]
	public async Task Login_AsActivatedUser_Should_GenerateValidJwtToken()
	{
		//arrange
		var passwordService = App.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerators.GenerateValidPassword();
		var user = UserGenerators.User
			.Clone()
			.WithPassword(passwordService.HashPassword(rawPassword))
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		var request = new LoginRequest
		{
			Email = user.Email,
			Password = rawPassword
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request, CancellationToken);

		//assert
		response.ShouldBe200OK();

		var token = await response.ReadFromJsonAsync<string>();
		token.ShouldNotBeNullOrEmpty();

		var handler = new JwtSecurityTokenHandler();
		var jwt = handler.ReadJwtToken(token);

		jwt.ShouldNotBeNull();
		jwt.ValidTo.Ticks.ShouldBeGreaterThan(DateTime.UtcNow.Ticks);
		jwt.Claims
			.Select(claim => (claim.Type, claim.Value))
			.ShouldContain([
				(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
				(ClaimTypes.Name, user.Name),
				(ClaimTypes.Email, user.Email)
			]);
	}

	[Fact]
	public async Task Login_AsInactivatedUser_Should_ResultInUnauthorized()
	{
		//arrange
		var passwordService = App.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerators.GenerateValidPassword();
		var user = UserGenerators.User
			.Clone()
			.WithPassword(passwordService.HashPassword(rawPassword))
			.WithStatus(UserStatus.NotActivated)
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		var request = new LoginRequest
		{
			Email = user.Email,
			Password = rawPassword
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request, CancellationToken);

		//assert
		response.ShouldBe401Unauthorized();

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
			Password = UserGenerators.GenerateValidPassword()
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request, CancellationToken);

		//assert
		response.ShouldBe401Unauthorized();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(AuthenticationErrors.InvalidCredentials);
	}

	[Fact]
	public async Task Login_WithIncorrectPassword_Should_ResultInUnauthorized()
	{
		//arrange
		var passwordService = App.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerators.GenerateValidPassword();
		var user = UserGenerators.User
			.Clone()
			.WithPassword(passwordService.HashPassword(rawPassword))
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Add(user);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		var request = new LoginRequest
		{
			Email = user.Email,
			Password = rawPassword + "x"
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request, CancellationToken);

		//assert
		response.ShouldBe401Unauthorized();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(AuthenticationErrors.InvalidCredentials);
	}
}
