namespace TeamUp.Tests.Architecture;

public sealed class DependencyTests : BaseTests
{
	private const string Common = "Common";
	private const string Domain = "Domain";
	private const string Application = "Application";
	private const string Infrastructure = "Infrastructure";
	private const string Presentation = "Api";
	private const string Contracts = "Contracts";

	[Theory]
	[InlineData(Domain)]
	[InlineData(Application)]
	[InlineData(Infrastructure)]
	[InlineData(Presentation)]
	[InlineData(Contracts)]
	public void Common_Should_NotHaveDependencyOn(string dependency)
	{
		var result = Types.InAssembly(DomainAssembly)
			.Should()
			.NotHaveDependencyOnAll(dependency)
			.GetResult();

		result.IsSuccessful.ShouldBeTrue();
	}

	[Theory]
	[InlineData(Application)]
	[InlineData(Infrastructure)]
	[InlineData(Presentation)]
	public void Domain_Should_NotHaveDependencyOn(string dependency)
	{
		var result = Types.InAssembly(DomainAssembly)
			.Should()
			.NotHaveDependencyOnAll(dependency)
			.GetResult();

		result.IsSuccessful.ShouldBeTrue();
	}

	[Theory]
	[InlineData(Infrastructure)]
	[InlineData(Presentation)]
	public void Application_Should_NotHaveDependencyOn(string dependency)
	{
		var result = Types.InAssembly(DomainAssembly)
			.Should()
			.NotHaveDependencyOnAll(dependency)
			.GetResult();

		result.IsSuccessful.ShouldBeTrue();
	}

	[Theory]
	[InlineData(Presentation)]
	public void Infrastructure_Should_NotHaveDependencyOn(string dependency)
	{
		var result = Types.InAssembly(DomainAssembly)
			.Should()
			.NotHaveDependencyOnAll(dependency)
			.GetResult();

		result.IsSuccessful.ShouldBeTrue();
	}

	[Theory]
	[InlineData(Common)]
	[InlineData(Domain)]
	[InlineData(Application)]
	[InlineData(Infrastructure)]
	[InlineData(Presentation)]
	public void Contracts_Should_NotHaveDependencyOn(string dependency)
	{
		var result = Types.InAssembly(DomainAssembly)
			.Should()
			.NotHaveDependencyOnAll(dependency)
			.GetResult();

		result.IsSuccessful.ShouldBeTrue();
	}
}
