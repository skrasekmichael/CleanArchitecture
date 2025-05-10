using Microsoft.EntityFrameworkCore;

namespace TeamUp.Tests.EndToEnd.EndpointTests.Teams;

public sealed class ChangeNicknameTests(AppFixture app) : TeamTests(app)
{
	public static string GetUrl(TeamId teamId) => GetUrl(teamId.Value);
	public static string GetUrl(Guid teamId) => $"/api/v1/teams/{teamId}/nickname";

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task ChangeNickname_ToValidNickname_AsTeamMember_Should_UpdateNicknameInDatabase(TeamRole teamRole)
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(initiatorUser, teamRole, members).Generate();

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
			Nickname = TeamGenerators.GenerateValidNickname()
		};

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.ShouldBe200OK();

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
		var owner = UserGenerators.User.Generate();
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();

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
			Nickname = TeamGenerators.GenerateValidNickname()
		};

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.ShouldBe403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task ChangeNickname_InTeamThatDoesNotExist_Should_ResultInNotFound()
	{
		//arrange
		var user = UserGenerators.User.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		var teamId = Guid.NewGuid();
		var request = new ChangeNicknameRequest
		{
			Nickname = TeamGenerators.GenerateValidNickname()
		};

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(teamId), request);

		//assert
		response.ShouldBe404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Theory]
	[InlineData("")]
	[InlineData("x")]
	[InlineData("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
	public async Task ChangeNickname_ToInvalidName_Should_ResultInBadRequest_ValidationErrors(string invalidName)
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(18);
		var team = TeamGenerators.Team.WithMembers(owner, members, (initiatorUser, TeamRole.Member)).Generate();

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
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id), request);

		//assert
		response.ShouldBe400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor(nameof(ChangeNicknameRequest.Nickname));
	}
}
