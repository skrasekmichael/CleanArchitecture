namespace TeamUp.Common.Abstraction;

public interface IDateTimeProvider
{
	public DateTime UtcNow { get; }

	public DateTimeOffset DateTimeOffsetNow { get; }
}
