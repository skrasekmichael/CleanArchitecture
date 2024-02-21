using Microsoft.EntityFrameworkCore;

using TeamUp.Contracts.Teams;

namespace TeamUp.EndToEndTests.EndpointTests.Teams;

public sealed class ChangeNicknameTests : BaseTeamTests
{
	public ChangeNicknameTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task ChangeNickname_ToValidNickname_AsTeamMember_Should_UpdateNicknameInDatabase(TeamRole teamRole)
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(initiatorUser, teamRole, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var targetMemberId = team.Members.First(member => member.UserId == initiatorUser.Id).Id;
		var request = new ChangeNicknameRequest
		{
			Nickname = TeamGenerator.GenerateValidNickname()
		};

		//act
		var response = await Client.PutAsJsonAsync($"/api/v1/teams/{team.Id.Value}/nickname", request);

		//assert
		response.Should().Be200Ok();

		var member = await UseDbContextAsync(dbContext =>
		{
			return dbContext
				.Set<TeamMember>()
				.SingleOrDefaultAsync(member => member.Id == targetMemberId);
		});

		member.ShouldNotBeNull();
		member.Nickname.Should().Be(request.Nickname);
	}

	[Fact]
	public async Task ChangeNickname_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(owner, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = new ChangeNicknameRequest
		{
			Nickname = TeamGenerator.GenerateValidNickname()
		};

		//act
		var response = await Client.PutAsJsonAsync($"/api/v1/teams/{team.Id.Value}/nickname", request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task ChangeNickname_InTeamThatDoesNotExist_Should_ResultInNotFound()
	{
		//arrange
		var user = UserGenerator.ActivatedUser.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		var teamId = F.Random.Guid();
		var request = new ChangeNicknameRequest
		{
			Nickname = TeamGenerator.GenerateValidNickname()
		};

		//act
		var response = await Client.PutAsJsonAsync($"/api/v1/teams/{teamId}/nickname", request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Theory]
	[InlineData("")]
	[InlineData("x")]
	[InlineData("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
	public async Task ChangeNickname_ToInvalidName_Should_ResultInBadRequest_ValidationErrors(string invalidName)
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(18);
		var team = TeamGenerator.GenerateTeamWith(owner, members, (initiatorUser, TeamRole.Member));

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var targetMemberId = team.Members.FirstOrDefault(member => member.UserId == initiatorUser.Id)!.Id;
		var request = new ChangeNicknameRequest
		{
			Nickname = invalidName
		};

		//act
		var response = await Client.PutAsJsonAsync($"/api/v1/teams/{team.Id.Value}/nickname", request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
		problemDetails.ShouldContainValidationErrorFor(nameof(ChangeNicknameRequest.Nickname));
	}
}
