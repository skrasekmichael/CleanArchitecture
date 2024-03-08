using System.Reflection;

using TeamUp.Application.Abstractions;

namespace TeamUp.Tests.Architecture.Application;

public sealed class AppQueryContextTests : BaseTests
{
	[Fact]
	public void AppQueryContext_Should_BeInjectedOnlyInQueryHandlers()
	{
		//arrange
		static bool ContainsAppQueryContextType(ConstructorInfo ctor)
			=> ctor.GetParameters().Any(param => param.ParameterType == typeof(IAppQueryContext));

		var appTypes = Types.InAssembly(ApplicationAssembly)
			.That()
			.DoNotImplementInterface(typeof(IQueryHandler<,>))
			.GetTypes();

		var failingTypes = new List<Type>();

		foreach (var type in appTypes)
		{
			var constructors = type.GetConstructors();
			if (constructors.Any(ContainsAppQueryContextType))
				failingTypes.Add(type);
		}

		//assert
		failingTypes.Should().BeEmpty();
	}
}
