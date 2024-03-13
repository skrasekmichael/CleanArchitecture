using TeamUp.Common;

namespace TeamUp.Tests.Common.DataGenerators;

public static class TeamGeneratorExtensions
{
	public static TeamGenerator WithMembers(this TeamGenerator teamGenerator, User user, TeamRole role, List<User> members)
	{
		var hasOwner = role == TeamRole.Owner || members.Count >= 1;
		hasOwner.Should().Be(true, "team has to have exactly 1 owner");

		return role switch
		{
			TeamRole.Owner => teamGenerator.GetTeamGeneratorWithMembers(members, (user, TeamRole.Owner)),
			_ => teamGenerator.GetTeamGeneratorWithMembers(members[1..], (members.First(), TeamRole.Owner), (user, role))
		};
	}

	public static TeamGenerator WithMembers(this TeamGenerator teamGenerator, User user1, TeamRole role1, User user2, TeamRole role2, List<User> members)
	{
		var hasOwner = role1 == TeamRole.Owner || role2 == TeamRole.Owner || members.Count >= 1;
		hasOwner.Should().Be(true, "team has to have exactly 1 owner");

		return (role1, role2) switch
		{
			(TeamRole.Owner, _) or (_, TeamRole.Owner) => teamGenerator.GetTeamGeneratorWithMembers(members, (user1, role1), (user2, role2)),
			_ => teamGenerator.GetTeamGeneratorWithMembers(members[1..], (members.First(), TeamRole.Owner), (user1, role1), (user2, role2))
		};
	}

	public static TeamGenerator WithMembers(this TeamGenerator teamGenerator, User owner, List<User> members, params (User User, TeamRole Role)[] userMembers)
	{
		var userMembersWithOwner = new (User User, TeamRole Role)[userMembers.Length + 1];
		userMembersWithOwner[0] = (owner, TeamRole.Owner);
		userMembers.CopyTo(userMembersWithOwner, 1);
		return teamGenerator.GetTeamGeneratorWithMembers(members, userMembersWithOwner);
	}

	public static TeamGenerator WithRandomMembers(this TeamGenerator teamGenerator, int count, List<User> pot)
	{
		count.Should().BeGreaterThan(0);
		pot.Should().HaveCountGreaterThanOrEqualTo(1);

		return teamGenerator
			.RuleFor(TeamGenerators.TEAM_MEMBERS_FIELD, (f, t) => f
				.Make(count, index => f.PopRandom(new List<User>(pot))
					.Map(user => TeamGenerators.TeamMember
						.RuleForBackingField(tm => tm.TeamId, t.Id)
						.RuleForBackingField(tm => tm.UserId, user.Id)
						.RuleFor(tm => tm.Nickname, user.Name)
						.RuleFor(tm => tm.Role, index == 1 ? TeamRole.Owner : TeamRole.Member)
						.Generate()))
				.ToHashSet()
				.ToList());
	}

	public static TeamGenerator WithMembers(this TeamGenerator teamGenerator, List<User> members, params (User User, TeamRole Role)[] userMembers)
	{
		userMembers.Where(x => x.Role == TeamRole.Owner).Should().ContainSingle("team has to have exactly 1 owner");
		members.Should().NotContain(userMembers.Select(x => x.User));
		return teamGenerator.GetTeamGeneratorWithMembers(members, userMembers);
	}

	private static TeamGenerator GetTeamGeneratorWithMembers(this TeamGenerator teamGenerator, List<User> members, params (User User, TeamRole Role)[] userMembers)
	{
		return teamGenerator
			.RuleFor(TeamGenerators.TEAM_MEMBERS_FIELD, (f, t) => userMembers
				.Select(um => TeamGenerators.TeamMember
					.RuleForBackingField(tm => tm.TeamId, t.Id)
					.RuleForBackingField(tm => tm.UserId, um.User.Id)
					.RuleFor(tm => tm.Nickname, um.User.Name)
					.RuleFor(tm => tm.Role, um.Role)
					.Generate())
				.Concat(members
					.Select(member => TeamGenerators.TeamMember
						.RuleForBackingField(tm => tm.TeamId, t.Id)
						.RuleForBackingField(tm => tm.UserId, member.Id)
						.RuleFor(tm => tm.Nickname, member.Name)
						.RuleFor(tm => tm.Role, TeamRole.Member)
						.Generate()))
				.ToList()
			);
	}

	public static TeamGenerator WithEventTypes(this TeamGenerator teamGenerator, int count)
	{
		return teamGenerator
			.RuleFor(TeamGenerators.TEAM_EVENTTYPES_FIELD, (f, t) => TeamGenerators.EventType
				.RuleForBackingField(et => et.TeamId, t.Id)
				.Generate(count));
	}
}
