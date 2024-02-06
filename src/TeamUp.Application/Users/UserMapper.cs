using Riok.Mapperly.Abstractions;

using TeamUp.Application.Users.Login;
using TeamUp.Application.Users.Register;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Public.Users;

namespace TeamUp.Application.Users;

[Mapper]
public sealed partial class UserMapper
{
	public partial RegisterUserCommand ToCommand(RegisterUserRequest request);

	public partial LoginCommand ToCommand(LoginRequest request);

	public partial UserResponse ToResponse(User user);
}
