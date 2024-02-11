namespace TeamUp.Common;

public static partial class ResultExtensions
{
	public static Result ToResult<TValue>(this Result<TValue> self)
	{
		if (self.IsFailure)
			return self.Error;

		return Result.Success;
	}

	public static async Task<Result> ToResultAsync<TValue>(this Task<Result<TValue>> selfTask)
	{
		return (await selfTask).ToResult();
	}

	public static Result<(TFirst, TSecond)> And<TFirst, TSecond>(this Result<TFirst> self, Func<TSecond> func)
	{
		if (self.IsFailure)
			return self.Error;

		return (self.Value!, func());
	}

	public static Result<(TFirst, TSecond)> And<TFirst, TSecond>(this Result<TFirst> self, Func<Result<TSecond>> func)
	{
		if (self.IsFailure)
			return self.Error;

		var result = func();
		if (result.IsFailure)
			return result.Error;

		return (self.Value!, result.Value!);
	}

	public static Result<(TFirst, TSecond)> And<TFirst, TSecond>(this Result<TFirst> self, Func<TFirst, Result<TSecond>> func)
	{
		if (self.IsFailure)
			return self.Error;

		var result = func(self.Value!);
		if (result.IsFailure)
			return result.Error;

		return (self.Value!, result.Value!);
	}
}
