namespace TeamUp.Domain.Abstractions;

public interface IUnitOfWork
{
	public Task<Result> SaveChangesAsync(CancellationToken ct = default);
}
