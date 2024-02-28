using Microsoft.EntityFrameworkCore;

using TeamUp.Contracts.Teams;

namespace TeamUp.EndToEndTests.EndpointTests.Teams;

public sealed class ChangeNicknameTests : BaseTeamTests
{
	public ChangeNicknameTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

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
		var initiatorUser = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(19);
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
		var owner = UserGenerators.ActivatedUser.Generate();
		var initiatorUser = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(19);
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
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task ChangeNickname_InTeamThatDoesNotExist_Should_ResultInNotFound()
	{
		//arrange
		var user = UserGenerators.ActivatedUser.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(user);

		var teamId = F.Random.Guid();
		var request = new ChangeNicknameRequest
		{
			Nickname = TeamGenerators.GenerateValidNickname()
		};

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(teamId), request);

		//assert
		response.Should().Be404NotFound();

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
		var owner = UserGenerators.ActivatedUser.Generate();
		var initiatorUser = UserGenerators.ActivatedUser.Generate();
		var members = UserGenerators.ActivatedUser.Generate(18);
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
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor(nameof(ChangeNicknameRequest.Nickname));
	}
}
