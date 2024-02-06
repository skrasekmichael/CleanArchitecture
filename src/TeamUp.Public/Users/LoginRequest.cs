using System.ComponentModel.DataAnnotations;

namespace TeamUp.Public.Users;

public sealed class LoginRequest
{
	[Required(AllowEmptyStrings = false)]
	[EmailAddress]
	[DataType(DataType.EmailAddress)]
	public required string Email { get; init; }

	[Required(AllowEmptyStrings = false)]
	[DataType(DataType.Password)]
	public required string Password { get; init; }
}
