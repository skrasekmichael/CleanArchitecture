namespace TeamUp.Common;

public static partial class ResultExtensions
{
	public static Result<TValue> Tap<TValue>(this Result<TValue> self, Action<TValue> func)
	{
		if (self.IsFailure)
			return self;

		func(self.Value);
		return self;
	}

	public static async Task<Result> TapAsync(this Result self, Func<Task> asyncFunc)
	{
		if (self.IsFailure)
			return self.Error;

		await asyncFunc();
		return Result.Success;
	}

	public static async Task<Result> TapAsync(this Task<Result> selfTask, Func<Task> asyncFunc)
	{
		var self = await selfTask;
		if (self.IsFailure)
			return self;

		await asyncFunc();
		return Result.Success;
	}

	public static async Task<Result<TValue>> TapAsync<TValue>(this Result<TValue> self, Func<TValue, Task> asyncFunc)
	{
		if (self.IsFailure)
			return self;

		await asyncFunc(self.Value);
		return self;
	}

	public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> selfTask, Func<TValue, Task> asyncFunc)
	{
		var self = await selfTask;
		return await self.TapAsync(asyncFunc);
	}

	public static async Task<Result<TValue>> Tap<TValue>(this Task<Result<TValue>> selfTask, Action<TValue> func)
	{
		var self = await selfTask;
		self.Tap(func);
		return self;
	}

	public static Result<(TFirst, TSecond)> Tap<TFirst, TSecond>(this Result<(TFirst, TSecond)> self, Action<TFirst, TSecond> func)
	{
		if (self.IsFailure)
			return self;

		func(self.Value.Item1, self.Value.Item2);
		return self;
	}

	public static async Task<Result<(TFirst, TSecond)>> Tap<TFirst, TSecond>(this Task<Result<(TFirst, TSecond)>> selfTask, Action<TFirst, TSecond> func)
	{
		var self = await selfTask;
		return self.Tap(func);
	}
}
