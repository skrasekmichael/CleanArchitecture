using TeamUp.Common;

namespace TeamUp.EndToEndTests.Extensions;

public static class ProblemDetailsExtensions
{
	public static void ShouldContainError<TError>(this ProblemDetails? details, TError error) where TError : ErrorBase
	{
		details.ShouldNotBeNull();
		details.Title.Should().Be(error.GetType().Name);
		details.Detail.Should().Be(error.Message);
	}

	public static void ShouldContainValidationErrorFor(this ValidationProblemDetails? details, params string[] names)
	{
		details.ShouldNotBeNull();
		details.Errors.Should().ContainKeys(names);
	}
}
