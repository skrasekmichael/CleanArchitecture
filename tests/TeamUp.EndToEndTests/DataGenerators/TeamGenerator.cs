using System.Linq;

using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

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

	public static string GenerateValidNickname() => F.Random.AlphaNumeric(10);

	public static readonly Faker<Team> EmptyTeam = new Faker<Team>(binder: TeamBinder)
		.UsePrivateConstructor()
		.RuleFor(t => t.Id, f => TeamId.FromGuid(f.Random.Uuid()))
		.RuleFor(t => t.Name, GenerateValidTeamName());

	public static readonly Faker<TeamMember> EmptyTeamMember = new Faker<TeamMember>(binder: TeamMemberBinder)
		.UsePrivateConstructor()
		.RuleFor(tm => tm.Id, f => TeamMemberId.FromGuid(f.Random.Uuid()));

	public static readonly Faker<UpsertEventTypeRequest> ValidUpsertEventTypeRequest = new Faker<UpsertEventTypeRequest>()
		.RuleFor(r => r.Name, f => f.Random.AlphaNumeric(10))
		.RuleFor(r => r.Description, f => f.Random.AlphaNumeric(40));

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

	public static Team GenerateTeamWith(User owner, List<User> members, params (User User, TeamRole Role)[] userMembers)
		=> GenerateTeamWith(members, userMembers.Concat([(owner, TeamRole.Owner)]).ToArray());

	public static Team GenerateTeamWith(List<User> members, params (User User, TeamRole Role)[] userMembers)
	{
		userMembers.Where(x => x.Role == TeamRole.Owner).Should().ContainSingle("team has to have exactly 1 owner");
		members.Should().NotContain(userMembers.Select(x => x.User));

		return EmptyTeam
			.RuleFor(TEAM_MEMBERS_FIELD, (f, t) =>
				userMembers.Select(um =>
					EmptyTeamMember
						.RuleForBackingField(tm => tm.TeamId, t.Id)
						.RuleForBackingField(tm => tm.UserId, um.User.Id)
						.RuleFor(tm => tm.Nickname, um.User.Name)
						.RuleFor(tm => tm.Role, um.Role)
						.Generate()
				).Concat(
					members.Select(member =>
						EmptyTeamMember
							.RuleForBackingField(tm => tm.TeamId, t.Id)
							.RuleForBackingField(tm => tm.UserId, member.Id)
							.RuleFor(tm => tm.Nickname, member.Name)
							.RuleFor(tm => tm.Role, TeamRole.Member)
							.Generate()
					)
				).ToList()
			)
			.Generate();
	}

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
