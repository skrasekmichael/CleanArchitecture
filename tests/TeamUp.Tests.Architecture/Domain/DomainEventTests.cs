using TeamUp.Domain.Abstractions;

namespace TeamUp.Tests.Architecture.Domain;

public sealed class DomainEventTests : BaseTests
{
	[Fact]
	public void DomainEvents_Should_BeSealed()
	{
		var result = Types.InAssembly(DomainAssembly)
			.That()
			.ImplementInterface(typeof(IDomainEvent))
			.Should()
			.BeSealed()
			.GetResult();

		result.IsSuccessful.ShouldBeTrue();
	}

	[Fact]
	public void DomainEvents_Should_HaveDomainEventSuffix()
	{
		var result = Types.InAssembly(DomainAssembly)
			.That()
			.ImplementInterface(typeof(IDomainEvent))
			.Should()
			.HaveNameEndingWith("DomainEvent")
			.GetResult();

		result.IsSuccessful.ShouldBeTrue();
	}
}
