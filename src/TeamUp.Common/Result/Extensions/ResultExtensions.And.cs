namespace TeamUp.Common;

public static partial class ResultExtensions
{
	public static Result<(TFirst, TSecond)> And<TFirst, TSecond>(this Result<TFirst> self, Func<TSecond> func)
	{
		if (self.IsFailure)
			return self.Error;

		return (self.Value, func());
	}

	public static Result<(TFirst, TSecond)> And<TFirst, TSecond>(this Result<TFirst> self, Func<Result<TSecond>> func)
	{
		if (self.IsFailure)
			return self.Error;

		var result = func();
		if (result.IsFailure)
			return result.Error;

		return (self.Value, result.Value);
	}

	public static Result<(TFirst, TSecond)> And<TFirst, TSecond>(this Result<TFirst> self, Func<TFirst, Result<TSecond>> func)
	{
		if (self.IsFailure)
			return self.Error;

		var result = func(self.Value);
		if (result.IsFailure)
			return result.Error;

		return (self.Value, result.Value);
	}

	public static async Task<Result<(TFirst, TSecond)>> AndAsync<TFirst, TSecond>(this Result<TFirst> self, Func<TFirst, Task<TSecond>> func)
	{
		if (self.IsFailure)
			return self.Error;

		var result = await func(self.Value);
		return (self.Value, result);
	}
}
