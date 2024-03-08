namespace TeamUp.Common;

public static class Extensions
{
	public static TResult Map<T, TResult>(this T source, Func<T, TResult> transform) => transform(source);
}
