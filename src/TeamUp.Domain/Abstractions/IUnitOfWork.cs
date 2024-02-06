namespace TeamUp.Domain.Abstractions;

public interface IUnitOfWork
{
	public Task SaveChangesAsync(CancellationToken ct = default);
}
