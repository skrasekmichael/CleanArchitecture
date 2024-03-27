namespace TeamUp.Tests.EndToEnd.Mocks;

internal abstract class BackgroundCallback
{
	private TaskCompletionSource<bool> _tcs = new();

	public void Invoke()
	{
		_tcs.SetResult(true);
		_tcs = new();
	}

	public async Task WaitForCallbackAsync() => await _tcs.Task;
}
