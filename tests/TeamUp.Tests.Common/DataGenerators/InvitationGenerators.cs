global using InvitationGenerator = Bogus.Faker<TeamUp.Domain.Aggregates.Invitations.Invitation>;

namespace TeamUp.Tests.Common.DataGenerators;

public sealed class InvitationGenerators : BaseGenerator
{
	private static readonly PrivateBinder InvitationBinder = new(
		nameof(Domain.Aggregates.Invitations.Invitation.RecipientId).GetBackingField(),
		nameof(Domain.Aggregates.Invitations.Invitation.TeamId).GetBackingField()
	);

	public static readonly InvitationGenerator Invitation = new InvitationGenerator(binder: InvitationBinder)
		.UsePrivateConstructor()
		.RuleFor(i => i.Id, f => InvitationId.New());

	public static Invitation GenerateInvitation(UserId userId, TeamId teamId, DateTime createdUtc)
	{
		return Invitation
			.RuleForBackingField(i => i.RecipientId, userId)
			.RuleForBackingField(i => i.TeamId, teamId)
			.RuleFor(i => i.CreatedUtc, createdUtc)
			.Generate();
	}

	public static List<Invitation> GenerateTeamInvitations(TeamId teamId, DateTime createdUtc, List<User> users)
	{
		var created = createdUtc.DropMicroSeconds();
		return users
			.Select(user =>
				Invitation
					.RuleForBackingField(i => i.RecipientId, user.Id)
					.RuleForBackingField(i => i.TeamId, teamId)
					.RuleFor(i => i.CreatedUtc, created)
					.Generate())
			.OrderBy(i => i.Id)
			.ToList();
	}

	public static List<Invitation> GenerateUserInvitations(UserId userId, DateTime createdUtc, List<Team> teams)
	{
		var created = createdUtc.DropMicroSeconds();
		return teams
			.Select(team =>
				Invitation
					.RuleForBackingField(i => i.RecipientId, userId)
					.RuleForBackingField(i => i.TeamId, team.Id)
					.RuleFor(i => i.CreatedUtc, created)
					.Generate())
			.OrderBy(i => i.Id)
			.ToList();
	}

	public static List<Invitation> GenerateRandomInvitations(DateTime createdUtc, List<User> users, List<Team> teams)
	{
		var created = createdUtc.DropMicroSeconds();
		return users
			.Select(user =>
				Invitation
					.RuleForBackingField(i => i.RecipientId, user.Id)
					.RuleForBackingField(i => i.TeamId, f => f.PickRandomFromReadOnlyList(teams).Id)
					.RuleFor(i => i.CreatedUtc, created)
					.Generate())
			.OrderBy(i => i.Id)
			.ToList();
	}

	public sealed class InvalidInviteUserRequest : TheoryData<InvalidRequest<InviteUserRequest>>
	{
		public InvalidInviteUserRequest()
		{
			this.Add(x => x.Email, new InviteUserRequest
			{
				TeamId = TeamId.New(),
				Email = ""
			});

			this.Add(x => x.Email, new InviteUserRequest
			{
				TeamId = TeamId.New(),
				Email = "@@"
			});

			this.Add(x => x.Email, new InviteUserRequest
			{
				TeamId = TeamId.New(),
				Email = "invalid email"
			});

			this.Add(x => x.Email, new InviteUserRequest
			{
				TeamId = TeamId.New(),
				Email = "missing.domain@"
			});

			this.Add(x => x.Email, new InviteUserRequest
			{
				TeamId = TeamId.New(),
				Email = "@missing.username"
			});
		}
	}
}
