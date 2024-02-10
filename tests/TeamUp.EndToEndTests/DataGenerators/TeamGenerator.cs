using TeamUp.EndToEndTests.Extensions;

namespace TeamUp.EndToEndTests.DataGenerators;

public sealed class TeamGenerator : BaseGenerator
{
	private static readonly PrivateBinder TeamBinder = new("_members");
	private static readonly PrivateBinder TeamMemberBinder = new(
		nameof(Domain.Aggregates.Teams.TeamMember.UserId).GetBackingField(),
		nameof(Domain.Aggregates.Teams.TeamMember.TeamId).GetBackingField()
	);

	public static string GenerateValidTeamName() => F.Random.AlphaNumeric(10);

	public static readonly Faker<Team> EmptyTeam = new Faker<Team>(binder: TeamBinder)
		.UsePrivateConstructor()
		.RuleFor(t => t.Id, f => TeamId.FromGuid(f.Random.Uuid()))
		.RuleFor(t => t.Name, GenerateValidTeamName());

	public static readonly Faker<TeamMember> TeamMember = new Faker<TeamMember>(binder: TeamMemberBinder)
		.UsePrivateConstructor()
		.RuleFor(tm => tm.Id, f => TeamMemberId.FromGuid(f.Random.Uuid()));

	public static Team GenerateTeamWithOwner(User owner)
	{
		return EmptyTeam
			.RuleFor("_members", (f, t) => new List<TeamMember> {
				TeamMember
					.RuleForBackingField(tm => tm.TeamId, t.Id)
					.RuleForBackingField(tm => tm.UserId, owner.Id)
					.RuleFor(tm => tm.Nickname, owner.Name)
					.RuleFor(tm => tm.Role, TeamRole.Owner)
					.Generate()
			})
			.Generate();
	}
}
