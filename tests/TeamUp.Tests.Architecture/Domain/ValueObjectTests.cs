using System.Reflection;

using TeamUp.Domain.Abstractions;

namespace TeamUp.Tests.Architecture.Domain;

public sealed class ValueObjectTests : BaseTests
{
	[Fact]
	public void ValueObjects_Should_HavePrivateParameterlessConstructor()
	{
		//arrange
		static bool IsPrivateParameterlessConstructor(ConstructorInfo ctor)
			=> ctor.IsPrivate && ctor.GetParameters().Length == 0;

		var valueObjectTypes = Types.InAssembly(DomainAssembly)
			.That()
			.ImplementInterface(typeof(IValueObject))
			.And()
			.AreNotAbstract()
			.GetTypes();

		var failingValueObjectTypes = new List<Type>();

		//act
		foreach (var valueObjectType in valueObjectTypes)
		{
			var constructors = valueObjectType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
			if (!constructors.Any(IsPrivateParameterlessConstructor))
				failingValueObjectTypes.Add(valueObjectType);
		}

		//assert
		failingValueObjectTypes.Should().BeEmpty();
	}
}
