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
	public TValue Value => IsSuccess ? _value! :
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
