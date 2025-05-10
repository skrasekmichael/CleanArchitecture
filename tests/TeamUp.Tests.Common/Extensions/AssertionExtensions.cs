using System.Diagnostics.CodeAnalysis;
using FluentAssertions.Primitives;

namespace TeamUp.Tests.Common.Extensions;

public static class AssertionExtensions
{
	public static AndConstraint<ObjectAssertions> ShouldNotBeNull<TObj>([NotNull] this TObj? obj)
	{
		return obj.Should().NotBeNull();
	}
}
