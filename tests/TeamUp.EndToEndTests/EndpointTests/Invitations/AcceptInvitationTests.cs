﻿using Microsoft.EntityFrameworkCore;

using TeamUp.Contracts.Invitations;
using TeamUp.Contracts.Teams;

namespace TeamUp.EndToEndTests.EndpointTests.Invitations;

public sealed class AcceptInvitationTests : BaseInvitationTests
{
	public AcceptInvitationTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	public static string GetUrl(InvitationId invitationId) => GetUrl(invitationId.Value);
	public static string GetUrl(Guid invitationId) => $"/api/v1/invitations/{invitationId}/accept";

	[Fact]
	public async Task AcceptInvitation_ThatIsValid_AsRecipient_Should_RemoveInvitationFromDatabase_And_AddUserAsMemberToTeamInDatabase()
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var owner = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(owner, members);
		var invitation = InvitationGenerator.GenerateInvitation(initiatorUser.Id, team.Id, DateTime.UtcNow);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, owner]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.Add(invitation);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.PostAsync(GetUrl(invitation.Id), null);

		//assert
		response.Should().Be200Ok();

		await UseDbContextAsync(async dbContext =>
		{
			var teamMembers = await dbContext
				.Set<TeamMember>()
				.Where(member => member.TeamId == team.Id)
				.ToListAsync();

			teamMembers.Should().HaveCount(21);

			var teamMember = teamMembers.SingleOrDefault(member => member.UserId == initiatorUser.Id);
			teamMember.ShouldNotBeNull();
			teamMember.Role.Should().Be(TeamRole.Member);

			var acceptedInvitation = await dbContext.Invitations.FindAsync(invitation.Id);
			acceptedInvitation.Should().BeNull();
		});
	}

	[Fact]
	public async Task AcceptInvitation_ThatExpired_AsRecipient_Should_ResultInBadRequest_DomainError()
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var owner = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(owner, members);
		var invitation = InvitationGenerator.GenerateInvitation(initiatorUser.Id, team.Id, DateTime.UtcNow.AddDays(-5));

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, owner]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.Add(invitation);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.PostAsync(GetUrl(invitation.Id), null);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(InvitationErrors.InvitationExpired);
	}

	[Fact]
	public async Task AcceptInvitation_ThatDoesNotExist_AsRecipient_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var owner = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(owner, members);
		var invitationId = F.Random.Guid();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, owner]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});
		Authenticate(initiatorUser);

		//act
		var response = await Client.PostAsync(GetUrl(invitationId), null);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(InvitationErrors.InvitationNotFound);
	}

	[Fact]
	public async Task AcceptInvitation_ForAnotherUser_Should_ResultInForbidden()
	{
		//arrange
		var initiatorUser = UserGenerator.ActivatedUser.Generate();
		var targetUser = UserGenerator.ActivatedUser.Generate();
		var owner = UserGenerator.ActivatedUser.Generate();
		var members = UserGenerator.ActivatedUser.Generate(19);
		var team = TeamGenerator.GenerateTeamWith(owner, members);
		var invitation = InvitationGenerator.GenerateInvitation(targetUser.Id, team.Id, DateTime.UtcNow.AddDays(-5));

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([initiatorUser, targetUser, owner]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Invitations.Add(invitation);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.PostAsync(GetUrl(invitation.Id), null);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(InvitationErrors.UnauthorizedToAcceptInvitation);
	}
}