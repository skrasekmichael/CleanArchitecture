using TeamUp.Contracts.Teams;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Tests.Common.DataGenerators;

namespace TeamUp.Tools.Seeder;

internal sealed class Seeder(ApplicationDbContext dbContext) : IAsyncDisposable, IDisposable
{
	private readonly ApplicationDbContext _dbContext = dbContext;

	public async Task SeedAsync(CancellationToken ct = default)
	{
		var users = UserGenerators.DistinctUser.Generate(3500);
		var usersWithoutTeam = UserGenerators.DistinctUser.Generate(500);

		_dbContext.Users.AddRange(users);
		_dbContext.Users.AddRange(usersWithoutTeam);

		Console.Write("Adding 4.000 users...");
		await _dbContext.SaveChangesAsync(ct);
		Console.WriteLine("DONE");

		var teams = TeamGenerators.Team
			.WithRandomMembers(TeamConstants.MAX_TEAM_CAPACITY, users)
			.WithEventTypes(10)
			.Generate(500);

		_dbContext.Teams.AddRange(teams);

		Console.Write("Adding 500 teams each with 30 members and 10 event types...");
		await _dbContext.SaveChangesAsync(ct);
		Console.WriteLine("DONE");

		var events = teams
			.Select(team => EventGenerators.Event
				.ForTeam(team.Id)
				.WithRandomEventType(team.EventTypes)
				.WithRandomEventResponses(team.Members)
				.Generate(100))
			.SelectMany(teamEvents => teamEvents)
			.ToList();

		_dbContext.Events.AddRange(events);

		Console.Write("Adding 50.000 events and 1.500.000 event responses...");
		await _dbContext.SaveChangesAsync(ct);
		Console.WriteLine("DONE");

		var invitations = InvitationGenerators.GenerateRandomInvitations(DateTime.UtcNow, usersWithoutTeam, teams);

		_dbContext.Invitations.AddRange(invitations);

		Console.Write("Adding 500 invitations...");
		await _dbContext.SaveChangesAsync(ct);
		Console.WriteLine("DONE");
	}

	public ValueTask DisposeAsync() => _dbContext.DisposeAsync();

	public void Dispose() => _dbContext.Dispose();
}
