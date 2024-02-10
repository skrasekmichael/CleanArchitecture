namespace TeamUp.EndToEndTests.DataGenerators;

public sealed class TeamGenerator : BaseGenerator
{
	private const string TEAM_MEMBERS_FIELD = "_members";

	private static readonly PrivateBinder TeamBinder = new(TEAM_MEMBERS_FIELD);
	private static readonly PrivateBinder TeamMemberBinder = new(
		nameof(TeamMember.UserId).GetBackingField(),
		nameof(TeamMember.TeamId).GetBackingField()
	);

	public static string GenerateValidTeamName() => F.Random.AlphaNumeric(10);

	public static readonly Faker<Team> EmptyTeam = new Faker<Team>(binder: TeamBinder)
		.UsePrivateConstructor()
		.RuleFor(t => t.Id, f => TeamId.FromGuid(f.Random.Uuid()))
		.RuleFor(t => t.Name, GenerateValidTeamName());

	public static readonly Faker<TeamMember> EmptyTeamMember = new Faker<TeamMember>(binder: TeamMemberBinder)
		.UsePrivateConstructor()
		.RuleFor(tm => tm.Id, f => TeamMemberId.FromGuid(f.Random.Uuid()));

	public static Team GenerateTeamWithOwner(User owner)
	{
		return EmptyTeam
			.RuleFor(TEAM_MEMBERS_FIELD, (f, t) => new List<TeamMember> {
				EmptyTeamMember
					.RuleForBackingField(tm => tm.TeamId, t.Id)
					.RuleForBackingField(tm => tm.UserId, owner.Id)
					.RuleFor(tm => tm.Nickname, owner.Name)
					.RuleFor(tm => tm.Role, TeamRole.Owner)
					.Generate()
			})
			.Generate();
	}
}
