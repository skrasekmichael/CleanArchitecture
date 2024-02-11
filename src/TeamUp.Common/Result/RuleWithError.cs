namespace TeamUp.Common;

public delegate bool Rule<in T>(T value);

public delegate bool Rule<in TFirst, in TSecond>(TFirst first, TSecond second);

public readonly struct RuleWithError<T> : IRuleWithError<T>
{
	public Rule<T> Condition { get; }
	public ErrorBase Error { get; }

	public RuleWithError(Rule<T> condition, ErrorBase error)
	{
		Condition = condition;
		Error = error;
	}

	public Result<T> Apply(T val) => Condition(val) ? val : Error;
}
