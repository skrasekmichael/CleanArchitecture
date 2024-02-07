using Riok.Mapperly.Abstractions;

using TeamUp.Application.Users.Login;
using TeamUp.Application.Users.Register;
using TeamUp.Contracts.Users;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Users;

[Mapper]
public sealed partial class UserMapper
{
	public partial RegisterUserCommand ToCommand(RegisterUserRequest request);

	public partial LoginCommand ToCommand(LoginRequest request);

	public partial UserResponse ToResponse(User user);
}
