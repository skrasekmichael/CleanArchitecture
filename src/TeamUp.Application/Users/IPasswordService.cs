﻿using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Users;

public interface IPasswordService
{
	public Password HashPassword(string password);
	public bool VerifyPassword(string inputRawPassword, Password dbPassword);
}
