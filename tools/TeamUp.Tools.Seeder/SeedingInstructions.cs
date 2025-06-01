using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using FluentValidation;
using TeamUp.Contracts.Teams;

namespace TeamUp.Tools.Seeder;

internal record class SeedingInstructions
{
	public int TotalUsers { get; init; } = 5000;
	public double UsersWithoutTeamRatio { get; init; } = 0.1;
	public int TotalTeams { get; init; } = 800;
	public int EventTypesPerTeam { get; init; } = 10;
	public int EventsPerTeam { get; init; } = 100;
	public double EventResponseRate { get; init; } = 0.8;
	public double InvitationsRate { get; init; } = 1.0;

	public int UsersWithTeam => TotalUsers - (int)(TotalUsers * UsersWithoutTeamRatio);
	private int PotentialNumberOfOwners => UsersWithTeam * TeamConstants.MAX_NUMBER_OF_OWNED_TEAMS;
	private int MinimumNumberOfTeamsForUsersWithTeams => UsersWithTeam / TeamConstants.MAX_TEAM_CAPACITY;

	internal static bool TryParse(string? input, [MaybeNullWhen(false)] out SeedingInstructions seedingInstructions)
	{
		if (input is null)
		{
			seedingInstructions = new SeedingInstructions();
			return true;
		}

		try
		{
			seedingInstructions = JsonSerializer.Deserialize<SeedingInstructions>(input);
			return seedingInstructions is not null;
		}
		catch
		{
			seedingInstructions = null;
			return false;
		}
	}

	internal sealed class SeedingInstructionsValidator : AbstractValidator<SeedingInstructions>
	{
		public SeedingInstructionsValidator()
		{
			RuleFor(x => x.TotalUsers)
				.GreaterThan(0)
				.WithMessage("Total users must be greater than 0.");

			RuleFor(x => x.UsersWithoutTeamRatio)
				.InclusiveBetween(0, 1);

			RuleFor(x => x.TotalTeams)
				.GreaterThanOrEqualTo(0)
				.WithMessage("Total number of teams must be a positive number.")
				.Must((model, totalTeams) => totalTeams >= model.MinimumNumberOfTeamsForUsersWithTeams)
				.WithMessage("Total number of teams must be greater or equal to minimum number of teams capable containing users with teams.")
				.Must((model, totalTeams) => totalTeams <= model.PotentialNumberOfOwners)
				.WithMessage("Total number of teams must be smaller or equal to total number of potential team owners.");

			RuleFor(x => x.EventTypesPerTeam)
				.GreaterThan(0).When((x, y) => x.EventsPerTeam > 0)
				.WithMessage("Number of event types per team must be greater than 0 when instructions are expecting event types.")
				.GreaterThanOrEqualTo(0)
				.WithMessage("Number of event types per team must be a positive number.");

			RuleFor(x => x.EventsPerTeam)
				.GreaterThanOrEqualTo(0)
				.WithMessage("Number of events per teams must be a positive number.");

			RuleFor(x => x.EventResponseRate)
				.InclusiveBetween(0, 1);

			RuleFor(x => x.InvitationsRate)
				.InclusiveBetween(0, 1);
		}
	}
}
