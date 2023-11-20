namespace TeamUp.Infrastructure.Processing;

public interface IDomainEventsDispatcher
{
	public Task DispatchDomainEventsAsync(CancellationToken ct = default);
}
