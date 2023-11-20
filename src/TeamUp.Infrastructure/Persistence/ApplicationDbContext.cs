using Microsoft.EntityFrameworkCore;

using TeamUp.Domain.Aggregates.Events;
using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Domain.SeedWork;

namespace TeamUp.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
	public required DbSet<User> Users { get; init; }
	public required DbSet<TeamMember> TeamMembers { get; init; }
	public required DbSet<Invitation> Invitations { get; init; }
	public required DbSet<Event> Events { get; init; }

	public ApplicationDbContext(DbContextOptions options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Ignore<List<IDomainEvent>>();
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

		base.OnModelCreating(modelBuilder);
	}
}
