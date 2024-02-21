namespace TeamUp.Common;

public static partial class ResultExtensions
{
	public static Result<TObj> Ensure<TObj, TError>(this TObj self, Rule<TObj> rule, TError error) where TError : ErrorBase
	{
		if (!rule(self))
			return error;

		return self;
	}

	public static Result<TObj> Ensure<TObj, TRule>(this TObj self, params TRule[] rules) where TRule : IRuleWithError<TObj>
	{
		foreach (var rule in rules)
		{
			var result = rule.Apply(self);
			if (result.IsFailure)
				return result;
		}

		return self;
	}

	public static Result<TValue> Ensure<TValue, TError>(this Result<TValue> self, Rule<TValue> rule, TError error) where TError : ErrorBase
	{
		if (self.IsFailure)
			return self;

		if (!rule(self.Value!))
			return error;

		return self;
	}

	public static Result<TValue> Ensure<TValue, TRule>(this Result<TValue> self, params TRule[] rules) where TRule : IRuleWithError<TValue>
	{
		if (self.IsFailure)
			return self;

		foreach (var rule in rules)
		{
			var result = rule.Apply(self.Value!);
			if (result.IsFailure)
				return result;
		}

		return self;
	}

	public static async Task<Result<TValue>> Ensure<TValue, TError>(this Task<Result<TValue>> selfTask, Rule<TValue> rule, TError error) where TError : ErrorBase
	{
		var self = await selfTask;
		return self.Ensure(rule, error);
	}

	public static Result<TObj> EnsureNotNull<TObj, TError>(this TObj? self, TError error) where TError : ErrorBase
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

	public static Result<TValue> EnsureNotNull<TValue, TProperty, TError>(this Result<TValue> self, Func<TValue, TProperty?> selector, TError error) where TError : ErrorBase
	{
		if (self.IsFailure)
			return self.Error;

		if (selector(self.Value!) is null)
			return error;

		return self.Value;
	}

	public static Result<(TFirst, TSecond)> Ensure<TFirst, TSecond, TError>(this Result<(TFirst, TSecond)> self, Rule<TFirst, TSecond> rule, TError error) where TError : ErrorBase
	{
		if (self.IsFailure)
			return self;

		if (!rule(self.Value.Item1, self.Value.Item2))
			return error;

		return self;
	}

	public static Result<(TFirst, TSecond)> Ensure<TFirst, TSecond, TRule>(this Result<(TFirst, TSecond)> self, TRule rule) where TRule : IRuleWithError<(TFirst, TSecond)>
	{
		if (self.IsFailure)
			return self;

		return rule.Apply(self.Value!);
	}

	public static async Task<Result<(TFirst, TSecond)>> Ensure<TFirst, TSecond, TError>(this Task<Result<(TFirst, TSecond)>> selfTask, Rule<TFirst, TSecond> rule, TError error) where TError : ErrorBase
	{
		var self = await selfTask;
		return self.Ensure(rule, error);
	}

	public static async Task<Result<(TFirst, TSecond)>> Ensure<TFirst, TSecond, TRule>(this Task<Result<(TFirst, TSecond)>> selfTask, TRule rule) where TRule : IRuleWithError<(TFirst, TSecond)>
	{
		var self = await selfTask;
		return self.Ensure(rule);
	}
}
