namespace TeamUp.Common;

public sealed class Result
{
	public bool IsSuccess { get; }
	public bool IsFailure => !IsSuccess;


	private readonly ErrorBase? _error = null;
	public ErrorBase Error => IsFailure ? _error! :
		throw new InvalidOperationException("Error of succeed result cannot be accessed.");

	public Result(ErrorBase error)
	{
		IsSuccess = false;
		_error = error;
	}

	private Result()
	{
		IsSuccess = true;
		_error = null;
	}

	public static readonly Result Success = new();

	public static implicit operator Result(ErrorBase error) => new(error);
}

public sealed class Result<TValue>
{
	public bool IsSuccess { get; }
	public bool IsFailure => !IsSuccess;

	private readonly TValue? _value;
	public TValue? Value => IsSuccess ? _value :
		throw new InvalidOperationException("Value of failure result cannot be accessed.");

	private readonly ErrorBase? _error = null;
	public ErrorBase Error => IsFailure ? _error! :
		throw new InvalidOperationException("Error of succeed result cannot be accessed.");

	private Result(ErrorBase error)
	{
		IsSuccess = false;
		_error = error;
		_value = default;
	}

	private Result(TValue? value)
	{
		IsSuccess = true;
		_error = null;
		_value = value;
	}

	public static Result<TValue> Success(TValue value) => new(value);

	public static implicit operator Result<TValue>(TValue? value) => new(value);
	public static implicit operator Result<TValue>(ErrorBase error) => new(error);
}

public static class ResultFunctionalExtensions
{
	public static Result<TObj> Ensure<TObj, TError>(this TObj self, Func<TObj, bool> func, TError error) where TError : ErrorBase
	{
		if (!func(self!))
			return error;

		return self;
	}

	public static Result<TValue> Ensure<TValue, TError>(this Result<TValue> self, Func<TValue, bool> func, TError error) where TError : ErrorBase
	{
		if (self.IsFailure)
			return self;

		if (!func(self.Value!))
			return error;

		return self;
	}

	public static Result<TObj> EnsureNotNull<TObj, TError>(this TObj? self, TError error)
		where TError : ErrorBase
	{
		if (self is null)
			return error;

		return self;
	}

	public static Result<TValue> EnsureNotNull<TValue, TError>(this Result<TValue?> self, TError error) where TError : ErrorBase
	{
		if (self.IsFailure)
			return self.Error;

		if (self.Value is null)
			return error;

		return self.Value;
	}

	public static Result<TValue> Tap<TValue>(this Result<TValue> self, Action<TValue> func)
	{
		if (self.IsFailure)
			return self;

		func(self.Value!);
		return self;
	}

	public static async Task<Result<TValue>> TapAsync<TValue>(this Result<TValue> self, Func<TValue, Task> asyncFunc)
	{
		if (self.IsFailure)
			return self;

		await asyncFunc(self.Value!);
		return self;
	}

	public static Result Then<TValue>(this Result<TValue> self, Action<TValue> func)
	{
		if (self.IsFailure)
			return self.Error;

		func(self.Value!);
		return Result.Success;
	}

	public static Result<TOut> Map<TValue, TOut>(this Result<TValue> self, Func<TValue, TOut> mapper)
	{
		if (self.IsFailure)
			return self.Error;

		return mapper(self.Value!);
	}

	public static Result<TOut> Map<TValue, TOut>(this Result<TValue> self, Func<TValue, Result<TOut>> mapper)
	{
		if (self.IsFailure)
			return self.Error;

		var result = mapper(self.Value!);
		if (result.IsFailure)
			return result.Error;

		return result.Value!;
	}

	public static Result<(TFirst, TSecond)> Ensure<TFirst, TSecond, TError>(this Result<(TFirst, TSecond)> self, Func<TFirst, TSecond, bool> func, TError error) where TError : ErrorBase
	{
		if (self.IsFailure)
			return self;

		if (!func(self.Value.Item1, self.Value.Item2))
			return error;

		return self;
	}

	public static Result<(TFirst, TSecond)> Tap<TFirst, TSecond>(this Result<(TFirst, TSecond)> self, Action<TFirst, TSecond> func)
	{
		if (self.IsFailure)
			return self;

		func(self.Value.Item1, self.Value.Item2);
		return self;
	}

	public static Result Then<TFirst, TSecond>(this Result<(TFirst, TSecond)> self, Action<TFirst, TSecond> func)
	{
		if (self.IsFailure)
			return self.Error;

		func(self.Value.Item1, self.Value.Item2);
		return Result.Success;
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

	public static Result<(TFirst, TSecond)> And<TFirst, TSecond>(this Result<TFirst> self, Func<TSecond> func)
	{
		if (self.IsFailure)
			return self.Error;

		return (self.Value!, func());
	}
}
