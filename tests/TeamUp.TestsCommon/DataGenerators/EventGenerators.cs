global using EventGenerator = Bogus.Faker<TeamUp.Domain.Aggregates.Events.Event>;
global using EventResponseGenerator = Bogus.Faker<TeamUp.Domain.Aggregates.Events.EventResponse>;

using FluentAssertions.Extensions;

namespace TeamUp.TestsCommon.DataGenerators;

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
		.RuleFor(e => e.Description, f => f.Random.Text(1, EventConstants.EVENT_DESCRIPTION_MAX_SIZE))
		.RuleFor(e => e.MeetTime, f => f.Date.Timespan(TimeSpan.FromHours(24)).DropMicroSeconds())
		.RuleFor(e => e.ReplyClosingTimeBeforeMeetTime, f => f.Date.Timespan(TimeSpan.FromHours(24)).DropMicroSeconds())
		.RuleFor(e => e.FromUtc, f => f.Date.Between(DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddMonths(6)).DropMicroSeconds().AsUtc())
		.RuleFor(e => e.ToUtc, (f, e) => e.FromUtc.AddHours(f.Random.Int(1, 5)).AsUtc())
		.RuleFor(e => e.Status, EventStatus.Open);

	public static readonly EventResponseGenerator Response = new EventResponseGenerator(binder: EventResponseBinder)
		.UsePrivateConstructor()
		.RuleFor(er => er.Id, f => EventResponseId.FromGuid(f.Random.Guid()));

	public static readonly Faker<CreateEventRequest> ValidCreateEventRequest = new Faker<CreateEventRequest>()
		.RuleFor(x => x.Description, f => f.Random.Text(1, EventConstants.EVENT_DESCRIPTION_MAX_SIZE))
		.RuleFor(x => x.MeetTime, f => f.Date.Timespan(TimeSpan.FromHours(24)).DropMicroSeconds())
		.RuleFor(x => x.ReplyClosingTimeBeforeMeetTime, f => f.Date.Timespan(TimeSpan.FromHours(24)).DropMicroSeconds());

	public static readonly Faker<UpsertEventReplyRequest> ValidUpsertEventReplyRequest = new Faker<UpsertEventReplyRequest>()
		.RuleFor(r => r.ReplyType, f => f.PickRandom(ReplyType.Yes, ReplyType.No, ReplyType.Maybe, ReplyType.Delay))
		.RuleFor(r => r.Message, (f, r) => r.ReplyType switch
		{
			ReplyType.Yes => string.Empty,
			ReplyType.Delay => f.Random.Text(0, EventConstants.EVENT_REPLY_MESSAGE_MAX_SIZE),
			ReplyType.Maybe or ReplyType.No or _ => f.Random.Text(1, EventConstants.EVENT_REPLY_MESSAGE_MAX_SIZE),
		});

	public sealed class InvalidCreateEventRequest : TheoryData<InvalidRequest<CreateEventRequest>>
	{
		public InvalidCreateEventRequest()
		{
			//empty description
			this.Add(x => x.Description, new CreateEventRequest
			{
				EventTypeId = EventTypeId.FromGuid(default),
				Description = "",
				MeetTime = TimeSpan.FromMinutes(15),
				ReplyClosingTimeBeforeMeetTime = TimeSpan.FromMinutes(15),
				FromUtc = DateTime.UtcNow.AddDays(1),
				ToUtc = DateTime.UtcNow.AddHours(25),
			});

			//negative meet time
			this.Add(x => x.MeetTime, new CreateEventRequest
			{
				EventTypeId = EventTypeId.FromGuid(default),
				Description = "xxx",
				MeetTime = TimeSpan.FromMinutes(-15),
				ReplyClosingTimeBeforeMeetTime = TimeSpan.FromMinutes(15),
				FromUtc = DateTime.UtcNow.AddDays(1),
				ToUtc = DateTime.UtcNow.AddHours(25),
			});

			//negative reply close time
			this.Add(x => x.ReplyClosingTimeBeforeMeetTime, new CreateEventRequest
			{
				EventTypeId = EventTypeId.FromGuid(default),
				Description = "xxx",
				MeetTime = TimeSpan.FromMinutes(15),
				ReplyClosingTimeBeforeMeetTime = TimeSpan.FromMinutes(-15),
				FromUtc = DateTime.UtcNow.AddDays(1),
				ToUtc = DateTime.UtcNow.AddHours(25),
			});

			//event starts (and ends) in past
			this.Add(x => x.FromUtc, new CreateEventRequest
			{
				EventTypeId = EventTypeId.FromGuid(default),
				Description = "xxx",
				MeetTime = TimeSpan.FromMinutes(15),
				ReplyClosingTimeBeforeMeetTime = TimeSpan.FromMinutes(15),
				FromUtc = DateTime.UtcNow.AddDays(-1),
				ToUtc = DateTime.UtcNow.AddHours(-23),
			});

			//event starts in past
			this.Add(x => x.FromUtc, new CreateEventRequest
			{
				EventTypeId = EventTypeId.FromGuid(default),
				Description = "xxx",
				MeetTime = TimeSpan.FromMinutes(15),
				ReplyClosingTimeBeforeMeetTime = TimeSpan.FromMinutes(15),
				FromUtc = DateTime.UtcNow.AddHours(-1),
				ToUtc = DateTime.UtcNow.AddHours(2),
			});

			//from > to
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

	public sealed class InvalidUpsertEventReplyRequest : TheoryData<InvalidRequest<UpsertEventReplyRequest>>
	{
		public InvalidUpsertEventReplyRequest()
		{
			//wrong type
			this.Add(x => x.ReplyType, new UpsertEventReplyRequest
			{
				ReplyType = (ReplyType)100,
				Message = "xxx"
			});

			//long message
			this.Add(x => x.Message, new UpsertEventReplyRequest
			{
				ReplyType = ReplyType.Yes,
				Message = "x"
			});

			//short message
			this.Add(x => x.Message, new UpsertEventReplyRequest
			{
				ReplyType = ReplyType.No,
				Message = ""
			});

			//short message
			this.Add(x => x.Message, new UpsertEventReplyRequest
			{
				ReplyType = ReplyType.Maybe,
				Message = ""
			});

			//long message
			this.Add(x => x.Message, new UpsertEventReplyRequest
			{
				ReplyType = ReplyType.Maybe,
				Message = F.Random.AlphaNumeric(EventConstants.EVENT_REPLY_MESSAGE_MAX_SIZE + 1)
			});

			//long message
			this.Add(x => x.Message, new UpsertEventReplyRequest
			{
				ReplyType = ReplyType.No,
				Message = F.Random.AlphaNumeric(EventConstants.EVENT_REPLY_MESSAGE_MAX_SIZE + 1)
			});

			//long message
			this.Add(x => x.Message, new UpsertEventReplyRequest
			{
				ReplyType = ReplyType.Delay,
				Message = F.Random.AlphaNumeric(EventConstants.EVENT_REPLY_MESSAGE_MAX_SIZE + 1)
			});
		}
	}
}
