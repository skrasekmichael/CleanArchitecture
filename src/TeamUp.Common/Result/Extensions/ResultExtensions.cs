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
}
