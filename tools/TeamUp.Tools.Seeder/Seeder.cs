using EFCore.BulkExtensions;
using TeamUp.Contracts.Teams;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Tests.Common.DataGenerators;
using TeamUp.Tools.Common;

namespace TeamUp.Tools.Seeder;

internal sealed class Seeder : IDisposable, IAsyncDisposable
{
	private readonly ApplicationDbContext _dbContext;
	private readonly SeedingInstructions _instructions;
	private readonly int _usersWithoutTeamCount, _usersWithTeamCount, _invitationsCount, _numberOfResponsesPerEvent;

	internal Seeder(ApplicationDbContext dbContext, SeedingInstructions instructions)
	{
		_dbContext = dbContext;
		_instructions = instructions;

		_usersWithoutTeamCount = (int)(instructions.TotalUsers * instructions.UsersWithoutTeamRatio);
		_usersWithTeamCount = instructions.TotalUsers - _usersWithoutTeamCount;
		_invitationsCount = (int)(instructions.InvitationsRate * _usersWithoutTeamCount);
		_numberOfResponsesPerEvent = (int)(instructions.EventResponseRate * instructions.EventsPerTeam);
	}

	public async Task SeedAsync(CancellationToken ct = default)
	{
		Console.WriteLine(_instructions);
		Console.WriteLine();

		var (users, usersWithoutTeam) = await SeedUsersAsync(ct);
		Console.WriteLine();

		var teams = await SeedTeamsAsync(users, ct);
		Console.WriteLine();

		await SeedEventsAsync(teams, ct);
		Console.WriteLine();

		await SeedInvitationsAsync(usersWithoutTeam, teams, ct);
		Console.WriteLine();
	}

	private async Task<(List<User> users, List<User> usersWithoutTeam)> SeedUsersAsync(CancellationToken ct)
	{
		Console.Write($"Generating {_instructions.TotalUsers} users...");
		var (users, usersWithoutTeam) = await ConsoleTimer.CallAsync(() =>
		{
			var users = UserGenerators.DistinctUser.Generate(_usersWithTeamCount);
			var usersWithoutTeam = UserGenerators.DistinctUser.Generate(_usersWithoutTeamCount);
			return (users, usersWithoutTeam);
		}, ct);

		_dbContext.Users.AddRange(users);
		_dbContext.Users.AddRange(usersWithoutTeam);

		Console.Write($"Inserting {_instructions.TotalUsers} users...");
		await ConsoleTimer.CallAsync(_dbContext.SaveChangesAsync, ct);
		Console.WriteLine($"Total users: {_instructions.TotalUsers}, users with no team: {_usersWithoutTeamCount}");
		return (users, usersWithoutTeam);
	}

	private async Task<List<Team>> SeedTeamsAsync(List<User> users, CancellationToken ct)
	{
		Console.Write($"Generating {_instructions.TotalTeams} teams...");
		var teams = await ConsoleTimer.CallAsync(() =>
		{
			return TeamGenerators.Team
				.WithRandomMembers(TeamConstants.MAX_TEAM_CAPACITY, users)
				.WithEventTypes(_instructions.EventTypesPerTeam)
				.Generate(_instructions.TotalTeams);
		}, ct);

		_dbContext.Teams.AddRange(teams);

		Console.Write($"Inserting {_instructions.TotalTeams} teams...");
		await ConsoleTimer.CallAsync(_dbContext.SaveChangesAsync, ct);
		Console.WriteLine($"Total teams: {_instructions.TotalTeams}, each with {TeamConstants.MAX_TEAM_CAPACITY} members and {_instructions.EventTypesPerTeam} event types");
		Console.WriteLine($"Total members: {TeamConstants.MAX_TEAM_CAPACITY * _instructions.TotalTeams}");
		Console.WriteLine($"Total event types: {_instructions.EventTypesPerTeam * _instructions.TotalTeams}");
		return teams;
	}

	private async Task SeedEventsAsync(List<Team> teams, CancellationToken ct)
	{
		Console.Write($"Generating {_instructions.EventsPerTeam * teams.Count} events...");
		var events = await ConsoleTimer.CallAsync(() =>
		{
			return teams
				.Select(team => EventGenerators.Event
					.ForTeam(team.Id)
					.WithRandomEventType(team.EventTypes)
					.WithRandomEventResponses(team.Members, _numberOfResponsesPerEvent)
					.Generate(_instructions.EventsPerTeam))
				.SelectMany(teamEvents => teamEvents)
				.ToList();
		}, ct);

		var bulkConfig = new BulkConfig
		{
			BatchSize = 50_000,
			SetOutputIdentity = false
		};

		Console.Write($"Inserting {events.Count} events...");
		await using (var timer = new ConsoleTimer(ct))
		{
			await _dbContext.BatchedInsertAsync(events, bulkConfig, timer.SetProgress, ct);
		}

		var eventResponses = events.SelectMany(e => e.EventResponses).ToList();
		Console.Write($"Inserting {eventResponses.Count} event responses...");
		await using (var timer = new ConsoleTimer(ct))
		{
			await _dbContext.BatchedInsertAsync(eventResponses, bulkConfig, timer.SetProgress, ct);
		}
	}

	private async Task SeedInvitationsAsync(List<User> usersWithoutTeam, List<Team> teams, CancellationToken ct)
	{
		Console.Write($"Generating {_invitationsCount} invitations...");
		var invitations = await ConsoleTimer.CallAsync(() =>
		{
			var usersToInvite = usersWithoutTeam.OrderBy(_ => Guid.NewGuid()).Take(_invitationsCount).ToList();
			return InvitationGenerators.GenerateRandomInvitations(DateTime.UtcNow, usersToInvite, teams);
		}, ct);

		_dbContext.Invitations.AddRange(invitations);

		Console.Write($"Inserting {invitations.Count} invitations...");
		await ConsoleTimer.CallAsync(_dbContext.SaveChangesAsync, ct);
	}

	public ValueTask DisposeAsync() => _dbContext.DisposeAsync();

	public void Dispose() => _dbContext.Dispose();
}
