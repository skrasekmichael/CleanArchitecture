using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Users;
using TeamUp.Common;

using EventResponse = TeamUp.Domain.Aggregates.Events.EventResponse;

namespace TeamUp.Tests.EndToEnd.EndpointTests.UserAccess;

public sealed class DeleteAccountTests(AppFixture app) : UserAccessTests(app)
{
	public const string URL = "/api/v1/users";

	[Fact]
	public async Task DeleteAccount_WithCorrectPassword_WhenUserIsNotTeamOwner_Should_DeleteUserAndAssociatedDataFromDatabase()
	{
		//arrange
		var passwordService = App.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerators.GenerateValidPassword();
		var targetUser = UserGenerators.User
			.WithPassword(passwordService.HashPassword(rawPassword))
			.Generate();

		var users = UserGenerators.User.Generate(49);
		users.Add(targetUser);

		var teams = TeamGenerators.Team
			.Clone()
			.WithRandomMembers(20, users, 1)
			.WithEventTypes(5)
			.Generate(5);

		var targetTeams = teams.Where(team => team.Members.All(m => m.UserId != targetUser.Id)).ToList();
		var targetInvitations = InvitationGenerators.GenerateUserInvitations(targetUser.Id, DateTime.UtcNow, targetTeams);

		var user = UserGenerators.User.Generate();
		users.Add(user);
		var expectedInvitations = InvitationGenerators.GenerateUserInvitations(user.Id, DateTime.UtcNow, teams);

		var events = teams
			.Select(team => EventGenerators.Event
				.Clone()
				.ForTeam(team.Id)
				.WithEventType(team.EventTypes[0].Id)
				.WithRandomEventResponses(team.Members)
				.Generate(5))
			.SelectMany(events => events)
			.OrderBy(e => e.Id)
			.ToList();

		var expectedTeamMembers = teams
			.SelectMany(team => team.Members)
			.Where(member => member.UserId != targetUser.Id)
			.OrderBy(member => member.Id);
		var expectedEventResponses = events
			.SelectMany(e => e.EventResponses)
			.Where(er => expectedTeamMembers.Any(member => member.Id == er.TeamMemberId))
			.OrderBy(er => er.Id)
			.ToList();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange(users);
			dbContext.Teams.AddRange(teams);
			dbContext.Events.AddRange(events);
			dbContext.Invitations.AddRange(targetInvitations);
			dbContext.Invitations.AddRange(expectedInvitations);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(targetUser);
		Client.DefaultRequestHeaders.Add(UserConstants.HTTP_HEADER_PASSWORD, rawPassword);

		//act
		var response = await Client.DeleteAsync(URL);

		//assert
		response.ShouldBe200OK();

		await UseDbContextAsync(async dbContext =>
		{
			//only targeted user should be deleted
			var notDeletedUsers = await dbContext.Users.OrderBy(u => u.Id).ToListAsync();
			notDeletedUsers.ShouldHaveSameValuesAs(users.Without(targetUser).OrderBy(u => u.Id));

			//only team members of target user should be deleted
			var notDeletedTeamMembers = await dbContext.Set<TeamMember>().OrderBy(m => m.Id).ToListAsync();
			notDeletedTeamMembers.ShouldHaveSameValuesAs(expectedTeamMembers);

			//only event responses of targeted user should be deleted
			var notDeletedEventResponses = await dbContext.Set<EventResponse>().OrderBy(er => er.Id).ToListAsync();
			notDeletedEventResponses.ShouldHaveSameValuesAs(expectedEventResponses);

			//only invitations for targeted user should be deleted
			var notDeletedInvitations = await dbContext.Invitations.OrderBy(i => i.Id).ToListAsync();
			notDeletedInvitations.ShouldHaveSameValuesAs(expectedInvitations);
		});
	}

	[Fact]
	public async Task DeleteAccount_WithCorrectPassword_WhenUserIsTeamOwnerAndOnlyMember_Should_DeleteUserAndAssociatedDataAndTeamFromDatabase()
	{
		//arrange
		var passwordService = App.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerators.GenerateValidPassword();
		var targetUser = UserGenerators.User
			.Clone()
			.WithPassword(passwordService.HashPassword(rawPassword))
			.Generate();

		var users = UserGenerators.User.Generate(49);

		var teams = TeamGenerators.Team
			.Clone()
			.WithRandomMembers(20, users.With(targetUser), 1)
			.WithEventTypes(5)
			.Generate(4);
		var targetTeam = TeamGenerators.Team
			.Clone()
			.WithOneOwner(targetUser)
			.WithEventTypes(5)
			.Generate();
		var allTeams = teams.With(targetTeam);

		var targetTeams = teams.Where(team => team.Members.Any(m => m.UserId == targetUser.Id));
		var nonTargetTeams = teams.Where(team => team.Members.All(m => m.UserId != targetUser.Id)).ToList();
		var targetInvitations = InvitationGenerators.GenerateUserInvitations(targetUser.Id, DateTime.UtcNow, nonTargetTeams);

		var user = UserGenerators.User.Generate();
		users.Add(user);
		var invitations = InvitationGenerators.GenerateUserInvitations(user.Id, DateTime.UtcNow, allTeams);

		var events = allTeams
			.Select(team => EventGenerators.Event
				.Clone()
				.ForTeam(team.Id)
				.WithEventType(team.EventTypes[0].Id)
				.WithRandomEventResponses(team.Members)
				.Generate(5))
			.SelectMany(events => events)
			.ToList();

		var expectedTeamMembers = teams
			.SelectMany(team => team.Members)
			.Where(member => member.UserId != targetUser.Id)
			.OrderBy(member => member.Id);
		var expectedEventResponses = events
			.SelectMany(e => e.EventResponses)
			.Where(er => expectedTeamMembers.Any(member => member.Id == er.TeamMemberId))
			.OrderBy(er => er.Id)
			.ToList();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(targetUser);
			dbContext.Users.AddRange(users);
			dbContext.Teams.AddRange(allTeams);
			dbContext.Events.AddRange(events);
			dbContext.Invitations.AddRange(targetInvitations);
			dbContext.Invitations.AddRange(invitations);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(targetUser);
		Client.DefaultRequestHeaders.Add(UserConstants.HTTP_HEADER_PASSWORD, rawPassword);

		//act
		var response = await Client.DeleteAsync(URL);

		//assert
		response.ShouldBe200OK();

		await UseDbContextAsync(async dbContext =>
		{
			//only targeted user should be deleted
			var notDeletedUsers = await dbContext.Users.OrderBy(u => u.Id).ToListAsync();
			notDeletedUsers.ShouldHaveSameValuesAs(users.OrderBy(u => u.Id));

			//only targeted team should be deleted
			var notDeletedTeams = await dbContext.Teams
				.Include(team => team.EventTypes.OrderBy(et => et.Id))
				.Include(team => team.Members.OrderBy(m => m.Id))
				.OrderBy(team => team.Id)
				.ToListAsync();
			notDeletedTeams.FirstOrDefault(t => t.Id == targetTeam.Id).ShouldBeNull();
			//not affected teams should stay the same
			notDeletedTeams.Pair<Team, TeamId>(nonTargetTeams).EachShouldHaveSameValues();
			//affected teams should have one less member
			notDeletedTeams.Pair<Team, TeamId>(targetTeams).ForEach(x =>
			{
				x.A.NumberOfMembers.ShouldBe(x.B.NumberOfMembers - 1);
				x.A.EventTypes.ShouldHaveSameValuesAs(x.B.EventTypes);
				x.A.Members.ShouldHaveSameValuesAs(x.B.Members.Where(m => m.UserId != targetUser.Id));
			});

			//only team members of target user should be deleted
			var notDeletedTeamMembers = await dbContext.Set<TeamMember>().OrderBy(m => m.Id).ToListAsync();
			notDeletedTeamMembers.ShouldHaveSameValuesAs(expectedTeamMembers);

			//only event responses of targeted user should be deleted
			var notDeletedEventResponses = await dbContext.Set<EventResponse>().OrderBy(r => r.Id).ToListAsync();
			notDeletedEventResponses.ShouldHaveSameValuesAs(expectedEventResponses);

			//invitations for targeted user or targeted team should be deleted
			var notDeletedInvitations = await dbContext.Invitations.OrderBy(i => i.Id).ToListAsync();
			notDeletedInvitations.ShouldHaveSameValuesAs(invitations.Where(invitation => invitation.TeamId != targetTeam.Id));
		});
	}

	[Fact]
	public async Task DeleteAccount_WithCorrectPassword_WhenUserIsTeamOwner_Should_DeleteUserAndAssociatedDataFromDatabase_And_ChangeOwnership()
	{
		//arrange
		var passwordService = App.Services.GetRequiredService<IPasswordService>();

		var rawPassword = UserGenerators.GenerateValidPassword();
		var targetUser = UserGenerators.User
			.Clone()
			.WithPassword(passwordService.HashPassword(rawPassword))
			.Generate();

		var users = UserGenerators.User.Generate(49);

		var teams = TeamGenerators.Team
			.Clone()
			.WithRandomMembers(25, users.With(targetUser), 1)
			.WithEventTypes(5)
			.Generate(4);
		var targetTeam = TeamGenerators.Team
			.Clone()
			.WithRandomMembers(25, targetUser.With(users), users.Count)
			.WithEventTypes(5)
			.Generate();
		var allTeams = teams.With(targetTeam);

		var targetTeams = teams.Where(team => team.Members.Any(m => m.UserId == targetUser.Id)).ToList();
		var nonTargetTeams = teams.Where(team => team.Members.All(m => m.UserId != targetUser.Id)).ToList();
		var targetInvitations = InvitationGenerators.GenerateUserInvitations(targetUser.Id, DateTime.UtcNow, nonTargetTeams);

		var user = UserGenerators.User.Generate();
		users.Add(user);
		var expectedInvitations = InvitationGenerators.GenerateUserInvitations(user.Id, DateTime.UtcNow, allTeams);

		var events = allTeams
			.Select(team => EventGenerators.Event
				.Clone()
				.ForTeam(team.Id)
				.WithEventType(team.EventTypes[0].Id)
				.WithRandomEventResponses(team.Members)
				.Generate(5))
			.SelectMany(events => events)
			.ToList();

		var expectedTeamMembers = allTeams
			.SelectMany(team => team.Members)
			.Where(member => member.UserId != targetUser.Id)
			.OrderBy(member => member.Id);
		var expectedEventResponses = events
			.SelectMany(e => e.EventResponses)
			.Where(er => expectedTeamMembers.Any(member => member.Id == er.TeamMemberId))
			.OrderBy(er => er.Id)
			.ToList();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(targetUser);
			dbContext.Users.AddRange(users);
			dbContext.Teams.AddRange(allTeams);
			dbContext.Events.AddRange(events);
			dbContext.Invitations.AddRange(targetInvitations);
			dbContext.Invitations.AddRange(expectedInvitations);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(targetUser);
		Client.DefaultRequestHeaders.Add(UserConstants.HTTP_HEADER_PASSWORD, rawPassword);

		//act
		var response = await Client.DeleteAsync(URL);

		//assert
		response.ShouldBe200OK();

		await UseDbContextAsync(async dbContext =>
		{
			//no teams should be deleted
			var notDeletedTeams = await dbContext.Teams
				.Include(team => team.Members.OrderBy(m => m.Id))
				.Include(team => team.EventTypes.OrderBy(et => et.Id))
				.ToListAsync();
			//not affected teams should remain the same
			notDeletedTeams.Pair<Team, TeamId>(teams.Where(t => t.Members.All(m => m.UserId != targetUser.Id))).EachShouldHaveSameValues();
			//affected teams should have 1 less member
			notDeletedTeams.Pair<Team, TeamId>(teams.Where(t => t.Members.Any(m => m.UserId == targetUser.Id))).ForEach(x =>
			{
				x.A.NumberOfMembers.ShouldBe(x.B.NumberOfMembers - 1);
				x.A.EventTypes.ShouldHaveSameValuesAs(x.B.EventTypes);
				x.A.Members.ShouldHaveSameValuesAs(x.B.Members.Where(m => m.UserId != targetUser.Id));
			});

			//target member should be removed from team and new owner should be in place
			var teamWithChangedOwner = notDeletedTeams.Single(team => team.Id == targetTeam.Id);
			var newOwner = teamWithChangedOwner.Members.Single(member => member.Role.IsOwner());
			teamWithChangedOwner.NumberOfMembers.ShouldBe(targetTeam.NumberOfMembers - 1);
			teamWithChangedOwner.Members.FirstOrDefault(m => m.UserId == targetUser.Id).ShouldBeNull();
			teamWithChangedOwner.Members.Pair<TeamMember, TeamMemberId>(targetTeam.Members.Where(m => m.UserId != newOwner.UserId)).EachShouldHaveSameValues();

			//only targeted user should be deleted
			var notDeletedUsers = await dbContext.Users.ToListAsync();
			notDeletedUsers.FirstOrDefault(u => u.Id == targetUser.Id).ShouldBeNull();
			//not affected users should stay the same
			notDeletedUsers.Pair<User, UserId>(users.Where(u => u.Id != newOwner.UserId)).EachShouldHaveSameValues();
			//the new owner only differs in number of owned teams
			var newOwnerUser = notDeletedUsers.Single(u => u.Id == newOwner.UserId);
			var oldOwnerUser = users.Single(u => u.Id == newOwner.UserId);
			newOwnerUser.NumberOfOwnedTeams.ShouldBe(oldOwnerUser.NumberOfOwnedTeams + 1);
			newOwnerUser.Name.ShouldBe(oldOwnerUser.Name);
			newOwnerUser.Email.ShouldBe(oldOwnerUser.Email);
			newOwnerUser.Status.ShouldBe(oldOwnerUser.Status);
			newOwnerUser.CreatedUtc.ShouldBe(oldOwnerUser.CreatedUtc);

			//only members of targeted user should be deleted
			var notDeletedTeamMembers = await dbContext.Set<TeamMember>().OrderBy(m => m.Id).ToListAsync();
			notDeletedTeamMembers.Pair<TeamMember, TeamMemberId>(expectedTeamMembers.Where(m => m.UserId != newOwner.UserId)).EachShouldHaveSameValues();
			notDeletedTeamMembers.FirstOrDefault(m => m.UserId == targetUser.Id).ShouldBeNull();
			notDeletedTeamMembers.Single(m => m.Id == newOwner.Id).Role.ShouldBe(TeamRole.Owner);

			//only responses by targeted user should be deleted
			var notDeletedEventResponses = await dbContext.Set<EventResponse>().OrderBy(r => r.Id).ToListAsync();
			notDeletedEventResponses.ShouldHaveSameValuesAs(expectedEventResponses);

			//only invitations for targeted user should be deleted
			var notDeletedInvitations = await dbContext.Invitations.OrderBy(i => i.Id).ToListAsync();
			notDeletedInvitations.ShouldHaveSameValuesAs(expectedInvitations);
		});
	}

	[Fact]
	public async Task DeleteAccount_WithInCorrectPassword_Should_ResultInUnauthorized()
	{
		//arrange
		var users = UserGenerators.User.Generate(50);
		var targetUser = F.PickRandom(users);

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange(users);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(targetUser);
		Client.DefaultRequestHeaders.Add(UserConstants.HTTP_HEADER_PASSWORD, "incorrect password");

		//act
		var response = await Client.DeleteAsync(URL);

		//assert
		response.ShouldBe401Unauthorized();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(AuthenticationErrors.InvalidCredentials);
	}

	[Fact]
	public async Task DeleteAccount_ThatDoesNotExist_Should_ResultInNotFound()
	{
		//arrange
		var users = UserGenerators.User.Generate(50);
		var targetUser = UserGenerators.User.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange(users);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(targetUser);
		Client.DefaultRequestHeaders.Add(UserConstants.HTTP_HEADER_PASSWORD, "whatever password");

		//act
		var response = await Client.DeleteAsync(URL);

		//assert
		response.ShouldBe404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UserErrors.AccountNotFound);
	}
}
