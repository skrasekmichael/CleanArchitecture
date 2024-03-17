using RailwayResult;
using RailwayResult.FunctionalExtensions;

namespace TeamUp.Common;

public static partial class ResultExtensions
{

	public static Result<TProperty> ThenEnsure<TValue, TProperty, TError>(this Result<TValue> self, Func<TValue, TProperty> selector, Rule<TProperty> rule, TError error) where TError : Error
	{
		if (self.IsFailure)
			return self.Error;

		var property = selector(self.Value);
		if (!rule(property))
			return error;

		return property;
	}

	public static Result<TProperty> ThenEnsure<TValue, TProperty, TRule>(this Result<TValue> self, Func<TValue, TProperty> selector, TRule rule) where TRule : IRuleWithError<TProperty>
	{
		if (self.IsFailure)
			return self.Error;

		return rule.Apply(selector(self.Value));
	}
}
