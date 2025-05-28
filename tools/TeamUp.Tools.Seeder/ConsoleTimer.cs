using System.Diagnostics;

namespace TeamUp.Tools.Seeder;

internal sealed class ConsoleTimer : IAsyncDisposable
{
	private readonly int _timerX, _timerY;
	private readonly long _startTimestamp;
	private readonly Task _timerTask;
	private double? _progress = null;
	private bool _isRunning;

	public ConsoleTimer(CancellationToken ct = default)
	{
		(_timerX, _timerY) = Console.GetCursorPosition();
		_isRunning = true;
		_startTimestamp = Stopwatch.GetTimestamp();

		_timerTask = Task.Run(async () =>
		{
			while (!ct.IsCancellationRequested)
			{
				var elapsed = Stopwatch.GetElapsedTime(_startTimestamp);

				Console.CursorVisible = false;
				var (currX, currY) = Console.GetCursorPosition();
				Console.SetCursorPosition(_timerX, _timerY);

				if (!_isRunning)
				{
					Console.WriteLine($"DONE [{elapsed:hh\\:mm\\:ss}]    ");
					return;
				}

				var progress = _progress is not null ? $"{_progress * 100,2:#0} % " : null;
				Console.Write($"{progress}[{elapsed:hh\\:mm\\:ss}]    ");
				Console.SetCursorPosition(currX, currY);
				Console.CursorVisible = true;
				await Task.Delay(1000, ct);
			}
		}, ct);
	}

	public void SetProgress(double progress)
	{
		_progress = progress;
	}

	public async ValueTask DisposeAsync()
	{
		_isRunning = false;
		await _timerTask;
	}

	public static Task<T> CallAsync<T>(Func<T> call, CancellationToken ct = default)
	{
		return CallAsync(ct => Task.Run(call, ct), ct);
	}

	public static async Task CallAsync(Func<CancellationToken, Task> asyncCall, CancellationToken ct = default)
	{
		await using var timer = new ConsoleTimer(ct);
		await asyncCall(ct);
	}

	public static async Task<T> CallAsync<T>(Func<CancellationToken, Task<T>> asyncCall, CancellationToken ct = default)
	{
		await using var timer = new ConsoleTimer(ct);
		return await asyncCall(ct);
	}
}
