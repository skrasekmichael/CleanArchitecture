namespace TeamUp.Common;

public static class Extensions
{
	public static TResult Map<T, TResult>(this T source, Func<T, TResult> transform) => transform(source);

	public static List<T> With<T>(this T first, List<T> list)
	{
		var newList = new List<T>(list.Count + 1)
		{
			first
		};
		newList.AddRange(list);
		return newList;
	}

	public static List<T> With<T>(this IEnumerable<T> list, T last) => new(list)
	{
		last
	};

	public static List<T> Without<T>(this IEnumerable<T> list, T element)
	{
		var newList = new List<T>(list);
		newList.Remove(element);
		return newList;
	}
}
