using TeamUp.Contracts.Abstractions;

namespace TeamUp.Tests.Architecture.Contracts;

public sealed class ClassesTests : BaseTests
{
	[Fact]
	public void Classes_Should_BeOneOf_Request_Response_Validator_TypedId_ClassWithConstants()
	{
		static bool IsResponse(Type type) => type.Name.EndsWith("Response");
		static bool IsRequest(Type type) => type.Name.EndsWith("Request");
		static bool IsValidator(Type type) => type.IsNested && type.Name == "Validator";
		static bool IsTypedId(Type type) => type.BaseType?.IsGenericType == true && type.BaseType == typeof(TypedId<>).MakeGenericType(type);
		static bool IsClassWithConstants(Type type) => type.IsAbstract && type.IsSealed && type.Name.EndsWith("Constants");

		var types = Types.InAssembly(ContractsAssembly)
			.That()
			.AreClasses()
			.GetTypes();

		var failingTypes = new List<Type>();

		foreach (var type in types)
		{
			var isAllowedType =
				IsResponse(type) ||
				IsRequest(type) ||
				IsValidator(type) ||
				IsTypedId(type) ||
				IsClassWithConstants(type) ||
				type.IsEnum ||
				type.IsAbstract;

			if (!isAllowedType)
				failingTypes.Add(type);
		}

		//assert
		failingTypes.ShouldBeEmpty();
	}
}
