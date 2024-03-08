namespace TeamUp.EndToEndTests.Mocks;

internal abstract class BackgroundCallback
{
	private TaskCompletionSource<bool> _tcs = new();

	public void Invoke()
	{
		_tcs.TrySetResult(true);
		_tcs = new();
	}

	public async Task WaitForCallbackAsync() => await _tcs.Task;
}

internal sealed class OutboxBackgroundCallback : BackgroundCallback;
