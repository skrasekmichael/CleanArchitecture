using Microsoft.EntityFrameworkCore;

using TeamUp.Domain.Aggregates.Events;
using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Domain.SeedWork;

namespace TeamUp.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
	public DbSet<User> Users => Set<User>();
	public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
	public DbSet<Invitation> Invitations => Set<Invitation>();
	public DbSet<Event> Events => Set<Event>();

	public ApplicationDbContext(DbContextOptions options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Ignore<List<IDomainEvent>>();
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

		base.OnModelCreating(modelBuilder);
	}
}
