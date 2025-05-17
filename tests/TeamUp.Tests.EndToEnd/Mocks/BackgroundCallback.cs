using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TeamUp.Tests.EndToEnd.Mocks;

internal abstract class BackgroundCallback
{
	private readonly Guid _identifier = Guid.CreateVersion7();
	private TaskCompletionSource<bool> _tcs = new();

	public void Invoke([CallerFilePath] string? source = null)
	{
		DebugLog(source, $"{_identifier} invoke");
		_tcs.SetResult(true);
		_tcs = new();
		DebugLog(source, $"{_identifier} invoke completed");
	}

	public async Task WaitForCallbackAsync([CallerFilePath] string? source = null)
	{
		DebugLog(source, $"{_identifier} waiting");
		await _tcs.Task;
		DebugLog(source, $"{_identifier} waiting completed");
	}

	[Conditional("DEBUG")]
	private void DebugLog(string? source, string message)
	{
		var caller = Path.GetFileNameWithoutExtension(source);
		Debug.WriteLine($"{GetType().Name} {caller} {message,-80}");
	}
}
