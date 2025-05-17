namespace TeamUp.Tests.EndToEnd.Mocks;

internal sealed class DelayedCommitUnitOfWorkOptions
{
	private bool _isDelayRequested = false;
	private readonly Lock _lock = new();

	public bool IsDelayRequested()
	{
		bool local;
		lock (_lock)
		{
			local = _isDelayRequested;
		}
		return local;
	}

	public void RequestDelay(bool val)
	{
		lock (_lock)
		{
			_isDelayRequested = val;
		}
	}
}
