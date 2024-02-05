using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using FluentAssertions;

using TeamUp.Api.Endpoints.UserAccess;
using TeamUp.Application.Users;

namespace TeamUp.EndToEndTests.EndpointTests;

public sealed class AuthTests : BaseEndpointTests
{
	public AuthTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	public Faker<RegisterUserRequest> ValidRegisterUserRequest = new Faker<RegisterUserRequest>()
		.RuleFor(r => r.Email, f => f.Internet.Email())
		.RuleFor(r => r.Name, f => f.Internet.UserName())
		.RuleFor(r => r.Password, f => f.Internet.Password(length: 10));

	[Fact]
	public async Task RegisterUser_Should_CreateNewUserInDatabase()
	{
		//arrange
		var request = ValidRegisterUserRequest.Generate();

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/users/register", request);

		//assert
		response.Should().Be201Created();

		var userId = await response.Content.ReadFromJsonAsync<UserId>(JsonSerializerOptions);
		userId.Should().NotBeNull();

		var user = await UseDbContextAsync(dbContext => dbContext.Users.FindAsync(userId));
		user.Should().NotBeNull();

		user!.Name.Should().BeEquivalentTo(request.Name);
		user.Email.Should().BeEquivalentTo(request.Email);
		user.Password.Should().NotBeEquivalentTo(request.Password);
	}

	[Fact]
	public async Task Login_AsExistingUser_Should_GenerateValidJwtToken()
	{
		//arrange
		var passwordService = AppFactory.Services.GetRequiredService<IPasswordService>();

		var rawPassword = "password";
		var user = User.Create(
			"Obi-Wan Kenobi",
			"Kenobi@email.com",
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

		jwt.Should().NotBeNull();
		jwt.ValidTo.Ticks.Should().BeGreaterThan(DateTime.UtcNow.Ticks);
		jwt.Claims.Select(claim => (claim.Type, claim.Value))
			.Should()
			.Contain([
				(JwtRegisteredClaimNames.Sub, user.Email),
				(ClaimTypes.NameIdentifier, user.Id.ToString()),
				(ClaimTypes.Name, user.Name),
				(ClaimTypes.Email, user.Email)
			]);
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
}
