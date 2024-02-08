
namespace TeamUp.Infrastructure.Processing;

public interface IIntegrationEventsDispatcher
{
	public Task DispatchIntegrationEventsAsync(CancellationToken ct = default);
}
