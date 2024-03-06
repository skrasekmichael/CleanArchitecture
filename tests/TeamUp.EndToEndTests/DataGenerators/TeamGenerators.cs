global using TeamGenerator = Bogus.Faker<TeamUp.Domain.Aggregates.Teams.Team>;

using TeamUp.Contracts.Teams;

namespace TeamUp.EndToEndTests.DataGenerators;

public sealed class TeamGenerators : BaseGenerator
{
	internal const string TEAM_MEMBERS_FIELD = "_members";
	internal static readonly List<TeamMember> EmptyTeamMemberList = [];

	internal const string TEAM_EVENTTYPES_FIELD = "_eventTypes";
	internal static readonly List<EventType> EmptyEventTypeList = [];

	private static readonly PrivateBinder TeamBinder = new(TEAM_MEMBERS_FIELD, TEAM_EVENTTYPES_FIELD);
	private static readonly PrivateBinder TeamMemberBinder = new(
		nameof(Domain.Aggregates.Teams.TeamMember.UserId).GetBackingField(),
		nameof(Domain.Aggregates.Teams.TeamMember.TeamId).GetBackingField()
	);
	private static readonly PrivateBinder EventTypeBinder = new(
		nameof(Domain.Aggregates.Teams.EventType.TeamId).GetBackingField()
	);

	public static readonly TeamGenerator Team = new TeamGenerator(binder: TeamBinder)
		.UsePrivateConstructor()
		.RuleFor(t => t.Id, f => TeamId.FromGuid(f.Random.Guid()))
		.RuleFor(t => t.Name, GenerateValidTeamName())
		.RuleFor(TEAM_EVENTTYPES_FIELD, f => EmptyEventTypeList)
		.RuleFor(TEAM_MEMBERS_FIELD, f => EmptyTeamMemberList);

	public static readonly Faker<TeamMember> TeamMember = new Faker<TeamMember>(binder: TeamMemberBinder)
		.UsePrivateConstructor()
		.RuleFor(tm => tm.Id, f => TeamMemberId.FromGuid(f.Random.Guid()));

	public static readonly Faker<EventType> EventType = new Faker<EventType>(binder: EventTypeBinder)
		.UsePrivateConstructor()
		.RuleFor(et => et.Id, f => EventTypeId.FromGuid(f.Random.Guid()))
		.RuleFor(et => et.Name, f => f.Random.Text(TeamConstants.EVENTTYPE_NAME_MIN_SIZE, TeamConstants.EVENTTYPE_NAME_MAX_SIZE))
		.RuleFor(et => et.Description, f => f.Random.Text(0, TeamConstants.EVENTTYPE_DESCRIPTION_MAX_SIZE));

	public static readonly Faker<UpsertEventTypeRequest> ValidUpsertEventTypeRequest = new Faker<UpsertEventTypeRequest>()
		.RuleFor(r => r.Name, f => f.Random.Text(TeamConstants.EVENTTYPE_NAME_MIN_SIZE, TeamConstants.EVENTTYPE_NAME_MAX_SIZE))
		.RuleFor(r => r.Description, f => f.Random.Text(0, TeamConstants.EVENTTYPE_DESCRIPTION_MAX_SIZE));

	public static string GenerateValidTeamName() => F.Random.Text(TeamConstants.TEAM_NAME_MIN_SIZE, TeamConstants.TEAM_NAME_MAX_SIZE);
	public static string GenerateValidNickname() => F.Random.Text(TeamConstants.NICKNAME_MIN_SIZE, TeamConstants.NICKNAME_MAX_SIZE);

	public sealed class InvalidUpsertEventTypeRequest : TheoryData<InvalidRequest<UpsertEventTypeRequest>>
	{
		public InvalidUpsertEventTypeRequest()
		{
			this.Add(x => x.Name, new UpsertEventTypeRequest
			{
				Name = "",
				Description = "xxx"
			});

			this.Add(x => x.Description, new UpsertEventTypeRequest
			{
				Name = "xxx",
				Description = ""
			});
		}
	}
}
