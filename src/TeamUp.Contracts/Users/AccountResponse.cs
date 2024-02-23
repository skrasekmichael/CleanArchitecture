namespace TeamUp.Contracts.Users;

public sealed class AccountResponse
{
	public required string Email { get; set; }
	public required string Name { get; set; }
	public required UserStatus Status { get; set; }
}
