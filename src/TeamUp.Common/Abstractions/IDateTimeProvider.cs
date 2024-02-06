namespace TeamUp.Common.Abstractions;

public interface IDateTimeProvider
{
	public DateTime UtcNow { get; }

	public DateTimeOffset DateTimeOffsetUtcNow { get; }
}
