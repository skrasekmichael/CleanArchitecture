namespace TeamUp.Tests.Performance.SmokeTests;

public sealed class EventsSmokeTests(AppFixture app, ITestOutputHelper output) : BasePerformanceTests(app, output)
{
	[Fact]
	public async Task GetEvents_SmokeTest()
	{
		//arrange
		var users = UserGenerators.DistinctUser.Generate(2000);
		var teams = TeamGenerators.Team
			.WithRandomMembers(30, users)
			.WithEventTypes(5)
			.Generate(100);
		var events = teams
			.Select(team => EventGenerators.Event
				.ForTeam(team.Id)
				.WithEventType(team.EventTypes[0].Id)
				.WithStatus(EventStatus.Open)
				.WithRandomEventResponses(team.Members)
				.Generate(50))
			.SelectMany(teamEvents => teamEvents)
			.ToList();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange(users);
			dbContext.Teams.AddRange(teams);
			dbContext.Events.AddRange(events);
			return dbContext.SaveChangesAsync();
		});

		var iterations = 8;
		var runs = new TimeSpan[iterations];

		//act
		for (int i = 0; i < iterations; i++)
		{
			var targetTeam = F.PickRandom(teams);
			var initiatorMember = F.PickRandomFromReadOnlyList(targetTeam.Members);
			var initiatorUser = users.First(user => user.Id == initiatorMember.UserId);

			Authenticate(initiatorUser);

			var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/teams/{targetTeam.Id.Value}/events");

			//act
			runs[i] = await RunTestAsync(request);

			Output.WriteLine($"{i}. iteration: {runs[i]} ({runs[i].TotalMilliseconds} ms)");
		}

		//assert
		var firstRun = runs[0];
		var avg = TimeSpan.FromTicks((long)runs[1..].Average(x => x.Ticks));

		Output.WriteLine($"average time without first run: {avg} ({avg.TotalMilliseconds} ms)");

		avg.ShouldBeLessThan(TimeSpan.FromMilliseconds(100));
		firstRun.ShouldBeLessThan(TimeSpan.FromMilliseconds(800));
	}

	[Fact]
	public async Task GetEvent_SmokeTest()
	{
		var users = UserGenerators.DistinctUser.Generate(2000);
		var teams = TeamGenerators.Team
			.WithRandomMembers(30, users)
			.WithEventTypes(5)
			.Generate(100);
		var events = teams
			.Select(team => EventGenerators.Event
				.ForTeam(team.Id)
				.WithEventType(team.EventTypes[0].Id)
				.WithStatus(EventStatus.Open)
				.WithRandomEventResponses(team.Members)
				.Generate(50))
			.SelectMany(teamEvents => teamEvents)
			.ToList();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange(users);
			dbContext.Teams.AddRange(teams);
			dbContext.Events.AddRange(events);
			return dbContext.SaveChangesAsync();
		});

		var iterations = 8;
		var runs = new TimeSpan[iterations];

		//act
		for (int i = 0; i < iterations; i++)
		{
			var targetTeam = F.PickRandom(teams);
			var initiatorMember = F.PickRandomFromReadOnlyList(targetTeam.Members);
			var initiatorUser = users.First(user => user.Id == initiatorMember.UserId);
			var targetEvent = F.PickRandom(events.Where(e => e.TeamId == targetTeam.Id));

			Authenticate(initiatorUser);

			var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/teams/{targetTeam.Id.Value}/events/{targetEvent.Id.Value}");

			//act
			runs[i] = await RunTestAsync(request);

			Output.WriteLine($"{i}. iteration: {runs[i]} ({runs[i].TotalMilliseconds} ms)");
		}

		//assert
		var firstRun = runs[0];
		var avg = TimeSpan.FromTicks((long)runs[1..].Average(x => x.Ticks));

		Output.WriteLine($"average time without first run: {avg} ({avg.TotalMilliseconds} ms)");

		avg.ShouldBeLessThan(TimeSpan.FromMilliseconds(100));
		firstRun.ShouldBeLessThan(TimeSpan.FromMilliseconds(1000));
	}
}
