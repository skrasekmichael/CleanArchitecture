using TeamUp.Contracts.Abstractions;
using TeamUp.Domain.Abstractions;

namespace TeamUp.Tests.Common.Extensions;

public static class CollectionExtensions
{
	public static IEnumerable<(T A, T B)> Pair<T, TId>(this IEnumerable<T> collectionA, IEnumerable<T> collectionB)
		where T : Entity<TId> where TId : TypedId<TId>, new()
	{
		foreach (var itemA in collectionA)
		{
			var itemB = collectionB.FirstOrDefault(x => x.Equals(itemA));
			if (itemB is not null)
			{
				yield return (itemA, itemB);
			}
		}
	}

	public static IEnumerable<(T A, T B)> PairAll<T, TId>(this IEnumerable<T> collectionA, IEnumerable<T> collectionB)
		where T : Entity<TId> where TId : TypedId<TId>, new()
	{
		foreach (var itemA in collectionA)
		{
			var itemB = collectionB.FirstOrDefault(x => x.Equals(itemA));
			itemB.ShouldNotBeNull();
			yield return (itemA, itemB);
		}
	}

	public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
	{
		foreach (var item in collection)
		{
			action(item);
		}
	}
}
