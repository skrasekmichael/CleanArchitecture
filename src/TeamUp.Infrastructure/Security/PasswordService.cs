using System.Security.Cryptography;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;

using TeamUp.Application.Users;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Infrastructure.Options;

namespace TeamUp.Infrastructure.Authentication;

internal sealed class PasswordService : IPasswordService
{
	private readonly IOptions<HashingOptions> _authentication;

	public PasswordService(IOptions<HashingOptions> authentication)
	{
		_authentication = authentication;
	}

	public Password HashPassword(string password)
	{
		var salt = RandomNumberGenerator.GetBytes(Password.SALT_SIZE);
		var hash = HashPassword(salt, password + _authentication.Value.Pepper);
		return new Password(salt, hash);
	}

	private byte[] HashPassword(byte[] salt, string password)
	{
		return KeyDerivation.Pbkdf2(
			password: password,
			salt: salt,
			prf: KeyDerivationPrf.HMACSHA512,
			iterationCount: _authentication.Value.Pbkdf2Iterations,
			numBytesRequested: Password.TOTAL_SIZE
		);
	}
}
