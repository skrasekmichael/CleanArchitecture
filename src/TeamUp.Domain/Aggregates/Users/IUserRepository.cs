﻿using TeamUp.Contracts.Users;

namespace TeamUp.Domain.Aggregates.Users;

public interface IUserRepository
{
	public Task<User?> GetUserByIdAsync(UserId id, CancellationToken ct = default);
	public Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default);
	public Task<bool> ExistsUserWithConflictingEmailAsync(string email, CancellationToken ct = default);
	public void AddUser(User user);
}
