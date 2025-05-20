using RailwayResult;

namespace TeamUp.Tests.EndToEnd.Extensions;

public static class ProblemDetailsExtensions
{
	public static void ShouldContainError<TError>(this ProblemDetails? details, TError error) where TError : Error
	{
		details.ShouldNotBeNull();
		details.Title.ShouldBe(error.GetType().Name);
		details.Detail.ShouldBe(error.Message);
	}

	public static void ShouldContainValidationErrorFor(this ValidationProblemDetails? details, params string[] names)
	{
		details.ShouldNotBeNull();
		foreach (var name in names)
		{
			details.Errors.ShouldContainKey(name);
		}
	}
}
