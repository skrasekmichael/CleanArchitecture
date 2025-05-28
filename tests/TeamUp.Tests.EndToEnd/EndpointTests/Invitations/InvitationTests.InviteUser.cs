
using Microsoft.EntityFrameworkCore;
using TeamUp.Infrastructure.Core;

namespace TeamUp.Tests.EndToEnd.EndpointTests.Invitations;

public sealed class InviteUserTests(AppFixture app) : InvitationTests(app)
{
	public const string URL = "/api/v1/invitations";

	[Theory]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task InviteUser_ThatIsActivated_AsCoordinatorOrHigher_Should_CreateInvitationInDatabase_And_SendInvitationEmail(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var targetUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(initiatorUser, initiatorRole, members).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request, CancellationToken);

		//assert
		response.ShouldBe201Created();

		var invitationId = await response.ReadFromJsonAsync<InvitationId>();
		invitationId.ShouldNotBeNull();

		var invitation = await UseDbContextAsync(dbContext => dbContext.Invitations.FindAsync(invitationId));
		invitation.ShouldNotBeNull();
		invitation.TeamId.ShouldBe(team.Id);
		invitation.RecipientId.ShouldBe(targetUser.Id);

		await WaitForIntegrationEventsAsync(); //wait for email

		Inbox.ShouldContain(email => email.EmailAddress == targetUser.Email);
	}

	[Theory]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task InviteUser_ThatIsNotRegistered_AsCoordinatorOrHigher_Should_CreateInvitationInDatabase_And_GenerateNewUser_And_SendInvitationEmail(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(initiatorUser, initiatorRole, members).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		var targetEmail = F.Internet.Email();
		var request = new InviteUserRequest
		{
			Email = targetEmail,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request, CancellationToken);

		//assert
		response.ShouldBe201Created();

		var invitationId = await response.ReadFromJsonAsync<InvitationId>();
		invitationId.ShouldNotBeNull();

		await UseDbContextAsync(async dbContext =>
		{
			var invitation = await dbContext.Invitations.FindAsync(invitationId);
			invitation.ShouldNotBeNull();
			invitation.TeamId.ShouldBe(team.Id);

			var user = await dbContext.Users.FindAsync(invitation.RecipientId);
			user.ShouldNotBeNull();
			user.Email.ShouldBe(targetEmail);
			user.Status.ShouldBe(UserStatus.Generated);
		});

		await WaitForIntegrationEventsAsync(); //wait for email

		Inbox.ShouldContain(email => email.EmailAddress == targetEmail);
	}

	[Theory]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task InviteUser_ThatIsAlreadyInTeam_AsCoordinatorOrHigher_Should_ResultInBadRequest_DomainError(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(initiatorUser, initiatorRole, members).Generate();
		var targetUser = members.First();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request, CancellationToken);

		//assert
		response.ShouldBe400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.CannotInviteUserThatIsTeamMember);
	}

	[Fact]
	public async Task InviteUser_AsTeamMember_Should_ResultInForbidden()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var targetUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(initiatorUser, TeamRole.Member, members).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request, CancellationToken);

		//assert
		response.ShouldBe403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToInviteTeamMembers);
	}

	[Theory]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task InviteUser_ThatIsAlreadyInvited_AsCoordinatorOrHigher_Should_ResultInConflict(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var targetUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(initiatorUser, initiatorRole, members).Generate();
		var invitation = InvitationGenerators.GenerateInvitation(targetUser.Id, team.Id, DateTime.UtcNow);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.Add(invitation);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request, CancellationToken);

		//assert
		response.ShouldBe409Conflict();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(InvitationErrors.UserIsAlreadyInvited);
	}

	[Fact]
	public async Task InviteUser_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var initiatorUser = UserGenerators.User.Generate();
		var targetUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request, CancellationToken);

		//assert
		response.ShouldBe403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task InviteUser_ToUnExistingTeam_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var targetUser = UserGenerators.User.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, targetUser]);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = TeamId.New()
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request, CancellationToken);

		//assert
		response.ShouldBe404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Theory]
	[ClassData(typeof(InvitationGenerators.InvalidInviteUserRequest))]
	public async Task InviteUser_WithInvalidRequest_Should_ResultInBadRequest_ValidationErrors(InvalidRequest<InviteUserRequest> request)
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange(initiatorUser);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.PostAsJsonAsync(URL, request.Request, CancellationToken);

		//assert
		response.ShouldBe400BadRequest();

		var problemDetails = await response.ReadValidationProblemDetailsAsync();
		problemDetails.ShouldContainValidationErrorFor(request.InvalidProperty);
	}

	[Fact]
	public async Task InviteUser_AsOwner_WhenConcurrentInvitationOfSameUserCompletes_Should_ResultInConflict()
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var targetUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(owner);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var (responseA, responseB) = await RunConcurrentRequestsAsync(
			() => Client.PostAsJsonAsync(URL, request, CancellationToken),
			() => Client.PostAsJsonAsync(URL, request, CancellationToken)
		);

		//assert
		responseA.ShouldBe201Created();
		responseB.ShouldBe409Conflict();

		var invitationId = await responseA.ReadFromJsonAsync<InvitationId>();
		invitationId.ShouldNotBeNull();

		await UseDbContextAsync(async dbContext =>
		{
			var invitation = await dbContext.Invitations.SingleAsync(invitation =>
				invitation.TeamId == team.Id &&
				invitation.RecipientId == targetUser.Id);
			invitation.Id.ShouldBe(invitationId);
		});

		var problemDetails = await responseB.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UnitOfWork.UniqueConstraintError);
	}

	[Fact]
	public async Task InviteUser_AsOwner_WhenTeamIsFull_Should_ResultInBadRequest_DomainError()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var targetUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(TeamConstants.MAX_TEAM_CAPACITY - 1);
		var team = TeamGenerators.Team.WithMembers(initiatorUser, members).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync(URL, request, CancellationToken);

		//assert
		response.ShouldBe400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.MaximumCapacityReached);
	}
}
