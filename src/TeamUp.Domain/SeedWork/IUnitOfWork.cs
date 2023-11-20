namespace TeamUp.Domain.SeedWork;

public interface IUnitOfWork
{
	Task SaveChangesAsync(CancellationToken ct = default);
}
