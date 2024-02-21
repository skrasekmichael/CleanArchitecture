using TeamUp.Contracts.Invitations;

namespace TeamUp.EndToEndTests.EndpointTests.Invitations;

public sealed class InviteUserTests : BaseInvitationTests
{
	public InviteUserTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	[Theory]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task InviteUser_ThatIsActivated_AsCoordinatorOrHigher_Should_CreateInvitationInDatabase_And_SendInvitationEmail(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var targetUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(initiatorUser, initiatorRole, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/invitations", request);

		//assert
		response.Should().Be201Created();

		var invitationId = await response.Content.ReadFromJsonAsync<InvitationId>();
		invitationId.ShouldNotBeNull();

		var invitation = await UseDbContextAsync(dbContext => dbContext.Invitations.FindAsync(invitationId));
		invitation.ShouldNotBeNull();
		invitation.TeamId.Should().Be(team.Id);
		invitation.RecipientId.Should().Be(targetUser.Id);

		await WaitForIntegrationEventsAsync(); //wait for email

		Inbox.Should().Contain(email => email.EmailAddress == targetUser.Email);
	}

	[Theory]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task InviteUser_ThatIsNotRegistered_AsCoordinatorOrHigher_Should_CreateInvitationInDatabase_And_GenerateNewUser_And_SendInvitationEmail(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(initiatorUser, initiatorRole, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var targetEmail = F.Internet.Email();
		var request = new InviteUserRequest
		{
			Email = targetEmail,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/invitations", request);

		//assert
		response.Should().Be201Created();

		var invitationId = await response.Content.ReadFromJsonAsync<InvitationId>();
		invitationId.ShouldNotBeNull();

		await UseDbContextAsync(async dbContext =>
		{
			var invitation = await dbContext.Invitations.FindAsync(invitationId);
			invitation.ShouldNotBeNull();
			invitation.TeamId.Should().Be(team.Id);

			var user = await dbContext.Users.FindAsync(invitation.RecipientId);
			user.ShouldNotBeNull();
			user.Email.Should().Be(targetEmail);
			user.Status.Should().Be(UserStatus.Generated);
		});

		await WaitForIntegrationEventsAsync(); //wait for email

		Inbox.Should().Contain(email => email.EmailAddress == targetEmail);
	}

	[Theory]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task InviteUser_ThatIsAlreadyInTeam_AsCoordinatorOrHigher_Should_ResultInBadRequest_DomainError(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(initiatorUser, initiatorRole, members);
		var targetUser = members.First();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/invitations", request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.CannotInviteUserThatIsTeamMember);
	}

	[Fact]
	public async Task InviteUser_AsTeamMember_Should_ResultInForbidden()
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var targetUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(initiatorUser, TeamRole.Member, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/invitations", request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToInviteTeamMembers);
	}

	[Theory]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	[InlineData(TeamRole.Owner)]
	public async Task InviteUser_ThatIsAlreadyInvited_AsCoordinatorOrHigher_Should_ResultInConflict(TeamRole initiatorRole)
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var targetUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(initiatorUser, initiatorRole, members);
		var invitation = InvitationGenerator.GenerateInvitation(targetUser.Id, team.Id, DateTime.UtcNow);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.Add(invitation);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/invitations", request);

		//assert
		response.Should().Be409Conflict();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.UserIsAlreadyInvited);
	}

	[Fact]
	public async Task InviteUser_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var owner = UserGenerator.ActivatedUser.Generate();
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var targetUser = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(owner, members);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser, targetUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = team.Id
		};

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/invitations", request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task InviteUser_ToUnExistingTeam_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var targetUser = UserGenerator.ActivatedUser.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, targetUser]);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = new InviteUserRequest
		{
			Email = targetUser.Email,
			TeamId = TeamId.FromGuid(F.Random.Guid())
		};

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/invitations", request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Theory]
	[ClassData(typeof(InvitationGenerator.InvalidInviteUserRequest))]
	public async Task InviteUser_WithInvalidRequest_Should_ResultInBadRequest_ValidationErrors(InvalidRequest<InviteUserRequest> request)
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange(initiatorUser);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.PostAsJsonAsync("/api/v1/invitations", request.Request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
		problemDetails.ShouldContainValidationErrorFor(request.InvalidProperty);
	}
}
