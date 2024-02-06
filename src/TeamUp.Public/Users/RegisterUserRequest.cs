using System.ComponentModel.DataAnnotations;

using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Public.Users;

public sealed class RegisterUserRequest
{
	[Required(AllowEmptyStrings = false)]
	[StringLength(User.NAME_MAX_SIZE, MinimumLength = User.NAME_MIN_SIZE)]
	[DataType(DataType.Text)]
	public required string Name { get; init; }

	[Required(AllowEmptyStrings = false)]
	[EmailAddress]
	[DataType(DataType.EmailAddress)]
	public required string Email { get; init; }

	[Required(AllowEmptyStrings = false)]
	[DataType(DataType.Password)]
	public required string Password { get; init; }
}
