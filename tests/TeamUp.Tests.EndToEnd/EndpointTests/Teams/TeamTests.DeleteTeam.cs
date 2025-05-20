using Microsoft.EntityFrameworkCore;
using TeamUp.Common;

namespace TeamUp.Tests.EndToEnd.EndpointTests.Teams;

public sealed class DeleteTeamTests(AppFixture app) : TeamTests(app)
{
	public static string GetUrl(TeamId teamId) => GetUrl(teamId.Value);
	public static string GetUrl(Guid teamId) => $"/api/v1/teams/{teamId}";

	[Fact]
	public async Task DeleteTeam_AsOwner_Should_DeleteTeamAndAssociatedDataInDatabase()
	{
		//arrange
		var users = UserGenerators.User.Generate(80);
		var teams = TeamGenerators.Team
			.WithRandomMembers(20, users)
			.WithEventTypes(5)
			.Generate(4)
			.OrderBy(t => t.Id)
			.ToList();

		var user = UserGenerators.User.Generate();
		users.Add(user);
		var invitations = InvitationGenerators.GenerateUserInvitations(user.Id, DateTime.UtcNow, teams);

		var teamEvents = teams.Select(team =>
		{
			return EventGenerators.Event
				.ForTeam(team.Id)
				.WithEventType(team.EventTypes[0].Id)
				.WithRandomEventResponses(team.Members)
				.Generate(15)
				.OrderBy(e => e.Id)
				.ToList();
		}).ToList();
		var events = teamEvents.SelectMany(events => events);

		var targetTeamIndex = F.Random.Int(0, teams.Count - 1);
		var targetTeam = teams[targetTeamIndex];
		var teamOwner = targetTeam.Members.Single(member => member.Role.IsOwner());
		var initiatorUser = users.Single(user => user.Id == teamOwner.UserId);

		var targetUsers = targetTeam.Members.Select(m => users.First(u => u.Id == m.UserId));

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange(users);
			dbContext.Teams.AddRange(teams);
			dbContext.Events.AddRange(events);
			dbContext.Invitations.AddRange(invitations);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.DeleteAsync(GetUrl(targetTeam.Id));

		//assert
		response.ShouldBe200OK();

		await UseDbContextAsync(async dbContext =>
		{
			//no users deleted and each affected user has one fewer team owned
			var notDeletedUsers = await dbContext.Users.OrderBy(u => u.Id).ToListAsync();
			notDeletedUsers.Pair<User, UserId>(users.Except([initiatorUser])).EachShouldHaveSameValues();
			var notDeletedUser = notDeletedUsers.Single(u => u.Id == initiatorUser.Id);
			notDeletedUser.NumberOfOwnedTeams.ShouldBe(initiatorUser.NumberOfOwnedTeams - 1);
			notDeletedUser.Name.ShouldBe(initiatorUser.Name);
			notDeletedUser.Email.ShouldBe(initiatorUser.Email);
			notDeletedUser.Status.ShouldBe(initiatorUser.Status);
			notDeletedUser.CreatedUtc.ShouldBe(initiatorUser.CreatedUtc);

			//only team, team members and event types from target team were deleted
			var notDeletedTeams = await dbContext.Teams
				.Include(team => team.Members.OrderBy(m => m.Id))
				.Include(team => team.EventTypes.OrderBy(et => et.Id))
				.OrderBy(team => team.Id)
				.ToListAsync();
			notDeletedTeams.ShouldHaveSameValuesAs(teams.Without(targetTeam).OrderBy(t => t.Id));

			//only events and event responses from target team were deleted
			var notDeletedEvents = await dbContext.Events
				.Include(e => e.EventResponses.OrderBy(er => er.TimeStampUtc))
				.OrderBy(e => e.Id)
				.ToListAsync();
			notDeletedEvents.ShouldHaveSameValuesAs(events.Except(teamEvents[targetTeamIndex]).OrderBy(e => e.Id));

			//only invitation to targeted team were deleted
			var notDeletedInvitations = await dbContext.Invitations.OrderBy(i => i.Id).ToListAsync();
			notDeletedInvitations.ShouldHaveSameValuesAs(invitations.Where(invitation => invitation.TeamId != targetTeam.Id).OrderBy(i => i.Id));
		});
	}

	[Theory]
	[InlineData(TeamRole.Member)]
	[InlineData(TeamRole.Coordinator)]
	[InlineData(TeamRole.Admin)]
	public async Task DeleteTeam_AsAdminOrLower_Should_ResultInForbidden(TeamRole teamRole)
	{
		//arrange
		var owner = UserGenerators.User.Generate();
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(18);
		var team = TeamGenerators.Team.WithMembers(owner, members, (initiatorUser, teamRole)).Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id));

		//assert
		response.ShouldBe403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.UnauthorizedToDeleteTeam);
	}

	[Fact]
	public async Task DeleteTeam_ThatDoesNotExist_Should_ResultInNotFound()
	{
		//arrange
		var user = UserGenerators.User.Generate();
		var teamId = Guid.NewGuid();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(user);
			return dbContext.SaveChangesAsync();
		});
		Authenticate(user);

		//act
		var response = await Client.DeleteAsync(GetUrl(teamId));

		//assert
		response.ShouldBe404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task DeleteTeam_WhenNotMemberOfTeam_Should_ResultInForbidden()
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

		//act
		var response = await Client.DeleteAsync(GetUrl(team.Id));

		//assert
		response.ShouldBe403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}
}
