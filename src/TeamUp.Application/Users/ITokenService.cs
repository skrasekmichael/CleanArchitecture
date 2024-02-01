using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Users;

public interface ITokenService
{
	public string GenerateToken(User user);
}
