using Riok.Mapperly.Abstractions;

using TeamUp.Application.Users.Login;
using TeamUp.Application.Users.Register;

namespace TeamUp.Api.Endpoints.UserAccess;

[Mapper]
public partial class UserMapper
{
	public partial RegisterUserCommand ToCommand(RegisterUserRequest request);

	public partial LoginCommand ToCommand(LoginRequest request);
}
