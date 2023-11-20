namespace TeamUp.Domain.Aggregates.Users;

public interface IUserRepository
{
	Task<User?> GetUserByIdAsync(UserId id, CancellationToken ct = default);
	Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default);
	Task AddUserAsync(User user, CancellationToken ct = default);
}
