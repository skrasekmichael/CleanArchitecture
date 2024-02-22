﻿using TeamUp.Contracts.Invitations;

namespace TeamUp.EndToEndTests.DataGenerators;

public sealed class InvitationGenerator : BaseGenerator
{
	private static readonly PrivateBinder InvitationBinder = new(
		nameof(Invitation.RecipientId).GetBackingField(),
		nameof(Invitation.TeamId).GetBackingField()
	);

	public static readonly Faker<Invitation> EmptyInvitation = new Faker<Invitation>(binder: InvitationBinder)
		.UsePrivateConstructor()
		.RuleFor(i => i.Id, f => InvitationId.FromGuid(f.Random.Guid()));

	public static Invitation GenerateInvitation(UserId userId, TeamId teamId, DateTime createdUtc)
	{
		return EmptyInvitation
			.RuleForBackingField(i => i.RecipientId, userId)
			.RuleForBackingField(i => i.TeamId, teamId)
			.RuleFor(i => i.CreatedUtc, createdUtc)
			.Generate();
	}

	public static List<Invitation> GenerateInvitations(TeamId teamId, DateTime createdUtc, List<User> users)
	{
		return users.Select(user =>
			EmptyInvitation
				.RuleForBackingField(i => i.RecipientId, user.Id)
				.RuleForBackingField(i => i.TeamId, teamId)
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