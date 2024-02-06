﻿using Microsoft.EntityFrameworkCore;

using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Infrastructure.Persistence.Domain.Users;

internal sealed class UserRepository : IUserRepository
{
	private readonly ApplicationDbContext _context;

	public UserRepository(ApplicationDbContext context)
	{
		_context = context;
	}

	public void AddUser(User user) => _context.Users.Add(user);

	public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
	{
		return await _context.Users
			.Where(user => user.Email == email)
			.FirstOrDefaultAsync(ct);
	}

	public async Task<User?> GetUserByIdAsync(UserId id, CancellationToken ct = default)
	{
		return await _context.Users.FindAsync([id], ct);
	}
}