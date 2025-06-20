﻿using Microsoft.EntityFrameworkCore;
using TeamUp.Infrastructure.Core;

namespace TeamUp.Tests.EndToEnd.EndpointTests.Invitations;

public sealed class AcceptInvitationTests(AppFixture app) : InvitationTests(app)
{
	public static string GetUrl(InvitationId invitationId) => GetUrl(invitationId.Value);
	public static string GetUrl(Guid invitationId) => $"/api/v1/invitations/{invitationId}/accept";

	[Fact]
	public async Task AcceptInvitation_ThatIsValid_AsRecipient_Should_RemoveInvitationFromDatabase_And_AddUserAsMemberToTeamInDatabase()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var owner = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();
		var invitation = InvitationGenerators.GenerateInvitation(initiatorUser.Id, team.Id, DateTime.UtcNow);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, owner]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.Add(invitation);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.PostAsync(GetUrl(invitation.Id), null, CancellationToken);

		//assert
		response.ShouldBe200OK();

		await UseDbContextAsync(async dbContext =>
		{
			var teamMembers = await dbContext
				.Set<TeamMember>()
				.Where(member => member.TeamId == team.Id)
				.ToListAsync();

			teamMembers.ShouldHaveCount(21);

			var teamMember = teamMembers.SingleOrDefault(member => member.UserId == initiatorUser.Id);
			teamMember.ShouldNotBeNull();
			teamMember.Role.ShouldBe(TeamRole.Member);

			var acceptedInvitation = await dbContext.Invitations.FindAsync(invitation.Id);
			acceptedInvitation.ShouldBeNull();
		});
	}

	[Fact]
	public async Task AcceptInvitation_ThatExpired_AsRecipient_Should_ResultInBadRequest_DomainError()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var owner = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();
		var invitation = InvitationGenerators.GenerateInvitation(initiatorUser.Id, team.Id, DateTime.UtcNow.AddDays(-5));

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, owner]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.Add(invitation);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.PostAsync(GetUrl(invitation.Id), null, CancellationToken);

		//assert
		response.ShouldBe400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(InvitationErrors.InvitationExpired);
	}

	[Fact]
	public async Task AcceptInvitation_ThatDoesNotExist_AsRecipient_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var owner = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();
		var invitationId = Guid.NewGuid();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, owner]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync(CancellationToken);
		});
		Authenticate(initiatorUser);

		//act
		var response = await Client.PostAsync(GetUrl(invitationId), null, CancellationToken);

		//assert
		response.ShouldBe404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(InvitationErrors.InvitationNotFound);
	}

	[Fact]
	public async Task AcceptInvitation_ForAnotherUser_Should_ResultInForbidden()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var targetUser = UserGenerators.User.Generate();
		var owner = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();
		var invitation = InvitationGenerators.GenerateInvitation(targetUser.Id, team.Id, DateTime.UtcNow.AddDays(-5));

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, targetUser, owner]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.Add(invitation);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.PostAsync(GetUrl(invitation.Id), null, CancellationToken);

		//assert
		response.ShouldBe403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(InvitationErrors.UnauthorizedToAcceptInvitation);
	}

	[Fact]
	public async Task AcceptInvitation_ThatIsValid_AsRecipient_WhenTeamIsFull_Should_ResultInBadRequest_DomainError()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var owner = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(TeamConstants.MAX_TEAM_CAPACITY - 1);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();
		var invitation = InvitationGenerators.GenerateInvitation(initiatorUser.Id, team.Id, DateTime.UtcNow);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, owner]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.Add(invitation);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.PostAsync(GetUrl(invitation.Id), null, CancellationToken);

		//assert
		response.ShouldBe400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.MaximumCapacityReached);
	}

	[Fact]
	public async Task AcceptInvitation_ThatIsValid_AsRecipient_ForLastEmptySpot_WhenConcurrentInvitationToSameTeamHasBeenAccepted_Should_ResultInConflict()
	{
		//arrange
		var userA = UserGenerators.User.Generate();
		var userB = UserGenerators.User.Generate();
		var owner = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(TeamConstants.MAX_TEAM_CAPACITY - 2);
		var team = TeamGenerators.Team.WithMembers(owner, members).Generate();
		var invitationA = InvitationGenerators.GenerateInvitation(userA.Id, team.Id, DateTime.UtcNow);
		var invitationB = InvitationGenerators.GenerateInvitation(userB.Id, team.Id, DateTime.UtcNow);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([userA, userB, owner]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.AddRange([invitationA, invitationB]);
			return dbContext.SaveChangesAsync(CancellationToken);
		});

		//act
		var (responseA, responseB) = await RunConcurrentRequestsAsync(
			() =>
			{
				Authenticate(userA);
				return Client.PostAsync(GetUrl(invitationA.Id), null, CancellationToken);
			},
			() =>
			{
				Authenticate(userB);
				return Client.PostAsync(GetUrl(invitationB.Id), null, CancellationToken);
			}
		);

		//assert
		responseA.ShouldBe200OK();
		responseB.ShouldBe409Conflict();

		await UseDbContextAsync(async dbContext =>
		{
			var teamMembers = await dbContext
				.Set<TeamMember>()
				.Where(member => member.TeamId == team.Id)
				.ToListAsync();

			teamMembers.ShouldHaveCount(TeamConstants.MAX_TEAM_CAPACITY);

			var teamMember = teamMembers.SingleOrDefault(member => member.UserId == userA.Id);
			teamMember.ShouldNotBeNull();
			teamMember.Role.ShouldBe(TeamRole.Member);

			var acceptedInvitation = await dbContext.Invitations.FindAsync(invitationA.Id);
			acceptedInvitation.ShouldBeNull();
		});

		var problemDetails = await responseB.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UnitOfWork.ConcurrencyError);
	}
}
