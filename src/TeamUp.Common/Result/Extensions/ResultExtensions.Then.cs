namespace TeamUp.Common;

public static partial class ResultExtensions
{
	public static Result<TOut> Then<TValue, TOut>(this Result<TValue> self, Func<TValue, TOut> mapper)
	{
		if (self.IsFailure)
			return self.Error;

		return mapper(self.Value!);
	}

	public static Result<TOut> Then<TValue, TOut>(this Result<TValue> self, Func<TValue, Result<TOut>> mapper)
	{
		if (self.IsFailure)
			return self.Error;

		return mapper(self.Value!);
	}

	public static async Task<Result<TOut>> ThenAsync<TValue, TOut>(this Result<TValue> self, Func<TValue, Task<TOut>> mapper)
	{
		if (self.IsFailure)
			return self.Error;

		return await mapper(self.Value!);
	}

	public static async Task<Result<TOut>> ThenAsync<TValue, TOut>(this Task<Result<TValue>> selfTask, Func<TValue, TOut> mapper)
	{
		var self = await selfTask;
		return self.Then(mapper);
	}

	public static Result Then<TFirst, TSecond>(this Result<(TFirst, TSecond)> self, Action<TFirst, TSecond> func)
	{
		if (self.IsFailure)
			return self.Error;

		func(self.Value.Item1, self.Value.Item2);
		return Result.Success;
	}

	public static Result<TOut> Then<TFirst, TSecond, TOut>(this Result<(TFirst, TSecond)> self, Func<TFirst, TSecond, Result<TOut>> mapper)
	{
		if (self.IsFailure)
			return self.Error;

		return mapper(self.Value.Item1, self.Value.Item2);
	}
}
