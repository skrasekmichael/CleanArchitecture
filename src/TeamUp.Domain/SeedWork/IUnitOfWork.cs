namespace TeamUp.Domain.SeedWork;

public interface IUnitOfWork
{
	public Task SaveChangesAsync(CancellationToken ct = default);
}
