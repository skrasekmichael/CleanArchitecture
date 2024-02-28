global using EventGenerator = Bogus.Faker<TeamUp.Domain.Aggregates.Events.Event>;
global using EventResponseGenerator = Bogus.Faker<TeamUp.Domain.Aggregates.Events.EventResponse>;

using FluentAssertions.Extensions;

using TeamUp.Contracts.Events;
using TeamUp.Contracts.Teams;

namespace TeamUp.EndToEndTests.DataGenerators;

public sealed class EventGenerators : BaseGenerator
{
	internal const string EVENT_RESPONSES_FIELD = "_eventResponses";
	private static readonly PrivateBinder EventBinder = new(
		EVENT_RESPONSES_FIELD,
		nameof(Domain.Aggregates.Events.Event.TeamId).GetBackingField()
	);

	private static readonly PrivateBinder EventResponseBinder = new(
		nameof(Domain.Aggregates.Events.EventResponse.EventId).GetBackingField(),
		nameof(Domain.Aggregates.Events.EventResponse.TeamMemberId).GetBackingField()
	);

	public static readonly EventGenerator Event = new EventGenerator(binder: EventBinder)
		.UsePrivateConstructor()
		.RuleFor(e => e.Id, f => EventId.FromGuid(f.Random.Guid()))
		.RuleFor(e => e.Description, f => f.Random.AlphaNumeric(20))
		.RuleFor(e => e.MeetTime, f => f.Date.Timespan(TimeSpan.FromHours(24)).DropMicroSeconds())
		.RuleFor(e => e.ReplyClosingTimeBeforeMeetTime, f => f.Date.Timespan(TimeSpan.FromHours(24)).DropMicroSeconds())
		.RuleFor(e => e.FromUtc, f => f.Date.Soon(7, DateTime.Now.AddDays(1)).DropMicroSeconds().AsUtc())
		.RuleFor(e => e.ToUtc, (f, e) => e.FromUtc.AddHours(f.Random.Int(1, 5)).AsUtc())
		.RuleFor(e => e.Status, EventStatus.Open);

	public static readonly EventResponseGenerator Response = new EventResponseGenerator(binder: EventResponseBinder)
		.UsePrivateConstructor()
		.RuleFor(er => er.Id, f => EventResponseId.FromGuid(f.Random.Guid()));

	public static readonly Faker<CreateEventRequest> ValidCreateEventRequest = new Faker<CreateEventRequest>()
		.RuleFor(x => x.Description, f => f.Random.AlphaNumeric(20))
		.RuleFor(x => x.MeetTime, f => f.Date.Timespan(TimeSpan.FromHours(24)).DropMicroSeconds())
		.RuleFor(x => x.ReplyClosingTimeBeforeMeetTime, f => f.Date.Timespan(TimeSpan.FromHours(24)).DropMicroSeconds());

	public sealed class InvalidCreateEventRequest : TheoryData<InvalidRequest<CreateEventRequest>>
	{
		public InvalidCreateEventRequest()
		{
			this.Add(x => x.Description, new CreateEventRequest
			{
				EventTypeId = EventTypeId.FromGuid(default),
				Description = "",
				MeetTime = TimeSpan.FromMinutes(15),
				ReplyClosingTimeBeforeMeetTime = TimeSpan.FromMinutes(15),
				FromUtc = DateTime.UtcNow.AddDays(1),
				ToUtc = DateTime.UtcNow.AddHours(25),
			});

			this.Add(x => x.MeetTime, new CreateEventRequest
			{
				EventTypeId = EventTypeId.FromGuid(default),
				Description = "xxx",
				MeetTime = TimeSpan.FromMinutes(-15),
				ReplyClosingTimeBeforeMeetTime = TimeSpan.FromMinutes(15),
				FromUtc = DateTime.UtcNow.AddDays(1),
				ToUtc = DateTime.UtcNow.AddHours(25),
			});

			this.Add(x => x.ReplyClosingTimeBeforeMeetTime, new CreateEventRequest
			{
				EventTypeId = EventTypeId.FromGuid(default),
				Description = "xxx",
				MeetTime = TimeSpan.FromMinutes(15),
				ReplyClosingTimeBeforeMeetTime = TimeSpan.FromMinutes(-15),
				FromUtc = DateTime.UtcNow.AddDays(1),
				ToUtc = DateTime.UtcNow.AddHours(25),
			});

			this.Add(x => x.FromUtc, new CreateEventRequest
			{
				EventTypeId = EventTypeId.FromGuid(default),
				Description = "xxx",
				MeetTime = TimeSpan.FromMinutes(15),
				ReplyClosingTimeBeforeMeetTime = TimeSpan.FromMinutes(15),
				FromUtc = DateTime.UtcNow.AddDays(-1),
				ToUtc = DateTime.UtcNow.AddHours(-23),
			});

			this.Add(x => x.FromUtc, new CreateEventRequest
			{
				EventTypeId = EventTypeId.FromGuid(default),
				Description = "xxx",
				MeetTime = TimeSpan.FromMinutes(15),
				ReplyClosingTimeBeforeMeetTime = TimeSpan.FromMinutes(15),
				FromUtc = DateTime.UtcNow.AddHours(-1),
				ToUtc = DateTime.UtcNow.AddHours(2),
			});

			this.Add(x => x.ToUtc, new CreateEventRequest
			{
				EventTypeId = EventTypeId.FromGuid(default),
				Description = "xxx",
				MeetTime = TimeSpan.FromMinutes(15),
				ReplyClosingTimeBeforeMeetTime = TimeSpan.FromMinutes(15),
				FromUtc = DateTime.UtcNow.AddHours(8),
				ToUtc = DateTime.UtcNow.AddHours(7),
			});
		}
	}
}
