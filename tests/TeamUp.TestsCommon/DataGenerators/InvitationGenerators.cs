global using InvitationGenerator = Bogus.Faker<TeamUp.Domain.Aggregates.Invitations.Invitation>;

namespace TeamUp.TestsCommon.DataGenerators;

public sealed class InvitationGenerators : BaseGenerator
{
	private static readonly PrivateBinder InvitationBinder = new(
		nameof(Domain.Aggregates.Invitations.Invitation.RecipientId).GetBackingField(),
		nameof(Domain.Aggregates.Invitations.Invitation.TeamId).GetBackingField()
	);

	public static readonly InvitationGenerator Invitation = new InvitationGenerator(binder: InvitationBinder)
		.UsePrivateConstructor()
		.RuleFor(i => i.Id, f => InvitationId.FromGuid(f.Random.Guid()));

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
		return users.Select(user =>
			Invitation
				.RuleForBackingField(i => i.RecipientId, user.Id)
				.RuleForBackingField(i => i.TeamId, teamId)
				.RuleFor(i => i.CreatedUtc, createdUtc)
				.Generate()
		).ToList();
	}

	public static List<Invitation> GenerateUserInvitations(UserId userId, DateTime createdUtc, List<Team> teams)
	{
		return teams.Select(team =>
			Invitation
				.RuleForBackingField(i => i.RecipientId, userId)
				.RuleForBackingField(i => i.TeamId, team.Id)
				.RuleFor(i => i.CreatedUtc, createdUtc)
				.Generate()
		).ToList();
	}

	public sealed class InvalidInviteUserRequest : TheoryData<InvalidRequest<InviteUserRequest>>
	{
		public InvalidInviteUserRequest()
		{
			this.Add(x => x.Email, new InviteUserRequest
			{
				TeamId = TeamId.FromGuid(F.Random.Guid()),
				Email = ""
			});

			this.Add(x => x.Email, new InviteUserRequest
			{
				TeamId = TeamId.FromGuid(F.Random.Guid()),
				Email = "@@"
			});

			this.Add(x => x.Email, new InviteUserRequest
			{
				TeamId = TeamId.FromGuid(F.Random.Guid()),
				Email = "invalid email"
			});

			this.Add(x => x.Email, new InviteUserRequest
			{
				TeamId = TeamId.FromGuid(F.Random.Guid()),
				Email = "missing.domain@"
			});

			this.Add(x => x.Email, new InviteUserRequest
			{
				TeamId = TeamId.FromGuid(F.Random.Guid()),
				Email = "@missing.username"
			});
		}
	}
}
