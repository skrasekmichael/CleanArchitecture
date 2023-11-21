namespace TeamUp.ArchitectureTests;

public sealed class DependencyTests : BaseTests
{
	private const string Domain = "Domain";
	private const string Application = "Application";
	private const string Infrastructure = "Infrastructure";
	private const string Presentation = "Api"; //TODOs

	[Theory]
	[InlineData(Domain)]
	[InlineData(Application)]
	[InlineData(Infrastructure)]
	[InlineData(Presentation)]
	public void Common_Should_NotHaveDependencyOn(string dependency)
	{
		var result = Types.InAssembly(DomainAssembly)
			.Should()
			.NotHaveDependencyOnAll(dependency)
			.GetResult();

		result.IsSuccessful.Should().BeTrue();
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

		result.IsSuccessful.Should().BeTrue();
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

		result.IsSuccessful.Should().BeTrue();
	}

	[Theory]
	[InlineData(Presentation)]
	public void Infrastructure_Should_NotHaveDependencyOn(string dependency)
	{
		var result = Types.InAssembly(DomainAssembly)
			.Should()
			.NotHaveDependencyOnAll(dependency)
			.GetResult();

		result.IsSuccessful.Should().BeTrue();
	}
}
