﻿using Riok.Mapperly.Abstractions;
using TeamUp.Application.Users.Login;
using TeamUp.Application.Users.RegisterUser;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Users;

[Mapper]
public sealed partial class UserMapper
{
	public partial RegisterUserCommand ToCommand(RegisterUserRequest request);

	public partial LoginCommand ToCommand(LoginRequest request);
}
