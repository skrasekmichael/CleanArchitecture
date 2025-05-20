using System.Collections;
using System.Reflection;

namespace TeamUp.Tests.Common.Extensions;

public static class ShouldExtensions
{
	public static void ShouldHaveCount<TItem>(this IReadOnlyCollection<TItem> collection, int expectedCount)
	{
		collection.Count.ShouldBe(expectedCount);
	}

	public static void ShouldHaveSingleItem<TItem>(this IEnumerable<TItem> collection, Func<TItem, bool> predicate)
	{
		collection.Where(predicate).ShouldHaveSingleItem();
	}

	public static void ShouldOnlyContain<TItem>(this IEnumerable<TItem> collection, Func<TItem, bool> predicate)
	{
		collection.Where(x => !predicate(x)).ShouldBeEmpty();
	}

	public static void ShouldContain<TItem>(this IEnumerable<TItem> collection, IEnumerable<TItem> inCollection)
	{
		foreach (var item in inCollection)
		{
			collection.ShouldContain(item);
		}
	}

	public static void ShouldNotContain<TItem>(this IEnumerable<TItem> collection, params IEnumerable<TItem> notInCollection)
	{
		foreach (var item in notInCollection)
		{
			collection.ShouldNotContain(item);
		}
	}

	public static void EachShouldHaveSameValues<TItem>(this IEnumerable<(TItem A, TItem B)> collection)
	{
		foreach (var item in collection)
		{
			item.A.ShouldHaveSameValuesAs(item.B);
		}
	}

	public static void ShouldHaveSameValuesAs(this object? objA, object? objB, int maxDepth = 5)
	{
		if (objA is null && objB is null)
			return;

		if (ReferenceEquals(objA, objB))
			return;

		objA.ShouldNotBeNull();
		var typeA = objA.GetType();

		objB.ShouldNotBeNull();
		var typeB = objB.GetType();

		if (typeA == typeB)
		{
			objA.ShouldBeEquivalentTo(objB);
			return;
		}

		maxDepth.ShouldBeGreaterThan(0, "max object comparison depth reached");

		if (objA is IEnumerable cA && objB is IEnumerable cB)
		{
			var enumA = cA.GetEnumerator();
			var enumB = cB.GetEnumerator();

			while (true)
			{
				var movedA = enumA.MoveNext();
				var movedB = enumB.MoveNext();
				if (!movedA && !movedB)
				{
					return;
				}

				movedA.ShouldBe(movedB);
				ShouldHaveSameValuesAs(enumA.Current, enumB.Current, maxDepth - 1);
			}
		}

		var propertiesA = typeA.GetProperties();
		var propertiesB = typeB.GetProperties();
		var sharedProperties = propertiesA
			.Select<PropertyInfo, (PropertyInfo A, PropertyInfo? B)>(a => (a, propertiesB.FirstOrDefault(b => b.Name == a.Name)))
			.Where(x => x.B is not null);

		foreach (var (propA, propB) in sharedProperties)
		{
			ShouldHaveSameValuesAs(propA.GetValue(objA), propB!.GetValue(objB), maxDepth - 1);
		}
	}
}
