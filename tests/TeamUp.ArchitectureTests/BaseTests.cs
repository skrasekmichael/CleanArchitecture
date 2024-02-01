using System.Reflection;

namespace TeamUp.ArchitectureTests;

public abstract class BaseTests
{
	public static readonly Assembly DomainAssembly = typeof(TeamUp.Domain.ServiceCollectionExtensions).Assembly;
	public static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.ServiceCollectionExtensions).Assembly;
}
