﻿using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Public.Users;

public sealed class UserResponse
{
	public required string Email { get; set; }
	public required string Name { get; set; }
	public required UserStatus Status { get; set; }
}