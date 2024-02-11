
namespace TeamUp.Common;

public interface IRuleWithError<T>
{
	public Result<T> Apply(T val);
}
