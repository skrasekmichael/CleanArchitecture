using System.Reflection;

using TeamUp.Domain.Abstractions;

namespace TeamUp.Tests.Architecture.Domain;

public sealed class EntityTests : BaseTests
{
	[Fact]
	public void Entities_Should_HavePrivateParameterlessConstructor()
	{
		//arrange
		static bool IsPrivateParameterlessConstructor(ConstructorInfo ctor)
			=> ctor.IsPrivate && ctor.GetParameters().Length == 0;

		var entityTypes = Types.InAssembly(DomainAssembly)
			.That()
			.Inherit(typeof(Entity<>))
			.And()
			.AreNotAbstract()
			.GetTypes();

		var failingEntityTypes = new List<Type>();

		foreach (var entityType in entityTypes)
		{
			var constructors = entityType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
			if (!constructors.Any(IsPrivateParameterlessConstructor))
				failingEntityTypes.Add(entityType);
		}

		//assert
		failingEntityTypes.ShouldBeEmpty();
	}
}
