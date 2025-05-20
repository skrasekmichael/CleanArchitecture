using FluentValidation;

using TeamUp.Contracts.Abstractions;

namespace TeamUp.Tests.Architecture.Contracts;

public sealed class RequestTests : BaseTests
{
	[Fact]
	public void Requests_Should_BeSealed_And_HaveRequestSuffix()
	{
		var result = Types.InAssembly(ContractsAssembly)
			.That()
			.ImplementInterface(typeof(IRequestBody))
			.Should()
			.BeSealed()
			.And()
			.HaveNameEndingWith("Request")
			.GetResult();

		result.IsSuccessful.ShouldBeTrue();
	}

	[Fact]
	public void Classes_WithRequestSuffix_Should_BeRequests()
	{
		var result = Types.InAssembly(ContractsAssembly)
			.That()
			.HaveNameEndingWith("Request")
			.Should()
			.ImplementInterface(typeof(IRequestBody))
			.GetResult();

		result.IsSuccessful.ShouldBeTrue();
	}

	[Fact]
	public void Requests_Should_HaveNestedSealedValidator()
	{
		static bool TypeIsValidatorForType(Type nestedType, Type requestType)
		{
			return nestedType.IsClass &&
				nestedType.IsSealed &&
				nestedType.Name == "Validator" &&
				nestedType.BaseType == typeof(AbstractValidator<>).MakeGenericType(requestType);
		}

		var requestTypes = Types.InAssembly(ContractsAssembly)
			.That()
			.ImplementInterface(typeof(IRequestBody))
			.GetTypes();

		var failingRequestTypes = new List<Type>();

		foreach (var requestType in requestTypes)
		{
			var nestedTypes = requestType.GetNestedTypes();
			if (nestedTypes.Length != 1 || !TypeIsValidatorForType(nestedTypes[0], requestType))
			{
				failingRequestTypes.Add(requestType);
				continue;
			}
		}

		//assert
		failingRequestTypes.ShouldBeEmpty();
	}
}
