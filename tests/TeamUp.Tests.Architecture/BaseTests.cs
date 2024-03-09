using System.Reflection;

namespace TeamUp.Tests.Architecture;

public abstract class BaseTests
{
	public static readonly Assembly DomainAssembly = typeof(TeamUp.Domain.ServiceCollectionExtensions).Assembly;
	public static readonly Assembly ApplicationAssembly = typeof(TeamUp.Application.ServiceCollectionExtensions).Assembly;
	public static readonly Assembly InfrastructureAssembly = typeof(TeamUp.Infrastructure.ServiceCollectionExtensions).Assembly;
	public static readonly Assembly ContractsAssembly = typeof(TeamUp.Contracts.Abstractions.IRequestBody).Assembly;
}
