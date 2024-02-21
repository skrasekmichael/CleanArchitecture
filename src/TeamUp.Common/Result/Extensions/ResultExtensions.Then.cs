namespace TeamUp.Common;

public static partial class ResultExtensions
{
	public static Result<TOut> Then<TValue, TOut>(this Result<TValue> self, Func<TValue, TOut> mapper)
	{
		if (self.IsFailure)
			return self.Error;

		return mapper(self.Value!);
	}

	public static Result Then<TValue>(this Result<TValue> self, Func<TValue, Result> mapper)
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

	public static async Task<Result<TOut>> ThenAsync<TValue, TOut>(this Result<TValue> self, Func<TValue, Task<TOut>> asyncMapper)
	{
		if (self.IsFailure)
			return self.Error;

		return await asyncMapper(self.Value!);
	}

	public static async Task<Result<TOut>> Then<TValue, TOut>(this Task<Result<TValue>> selfTask, Func<TValue, TOut> mapper)
	{
		var self = await selfTask;
		return self.Then(mapper);
	}

	public static Result<TOut> Then<TFirst, TSecond, TOut>(this Result<(TFirst, TSecond)> self, Func<TFirst, TSecond, TOut> mapper)
	{
		if (self.IsFailure)
			return self.Error;

		return mapper(self.Value.Item1, self.Value.Item2);
	}

	public static Result<TOut> Then<TFirst, TSecond, TOut>(this Result<(TFirst, TSecond)> self, Func<TFirst, TSecond, Result<TOut>> mapper)
	{
		if (self.IsFailure)
			return self.Error;

		return mapper(self.Value.Item1, self.Value.Item2);
	}

	public static async Task<Result<TOut>> Then<TFirst, TSecond, TOut>(this Task<Result<(TFirst, TSecond)>> selfTask, Func<TFirst, TSecond, TOut> mapper)
	{
		var self = await selfTask;
		return self.Then(mapper);
	}

	public static async Task<Result<TOut>> ThenAsync<TFirst, TSecond, TOut>(this Result<(TFirst, TSecond)> self, Func<TFirst, TSecond, Task<TOut>> asyncMapper)
	{
		if (self.IsFailure)
			return self.Error;

		return await asyncMapper(self.Value!.Item1, self.Value.Item2);
	}

	public static async Task<Result<TOut>> ThenAsync<TFirst, TSecond, TOut>(this Task<Result<(TFirst, TSecond)>> selfTask, Func<TFirst, TSecond, Task<TOut>> asyncMapper)
	{
		var self = await selfTask;
		return await self.ThenAsync(asyncMapper);
	}

	public static async Task<Result<TOut>> ThenAsync<TFirst, TSecond, TOut>(this Result<(TFirst, TSecond)> self, Func<TFirst, TSecond, Task<Result<TOut>>> asyncMapper)
	{
		if (self.IsFailure)
			return self.Error;

		return await asyncMapper(self.Value!.Item1, self.Value.Item2);
	}

	public static async Task<Result<TOut>> ThenAsync<TFirst, TSecond, TOut>(this Task<Result<(TFirst, TSecond)>> selfTask, Func<TFirst, TSecond, Task<Result<TOut>>> asyncMapper)
	{
		var self = await selfTask;
		return await self.ThenAsync(asyncMapper);
	}
}
