using System.Reflection;

namespace TeamUp.ArchitectureTests;

public abstract class BaseTests
{
	public static readonly Assembly DomainAssembly = typeof(TeamUp.Domain.SeedWork.Entity<>).Assembly;
	public static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.Persistence.ApplicationDbContext).Assembly;
}
