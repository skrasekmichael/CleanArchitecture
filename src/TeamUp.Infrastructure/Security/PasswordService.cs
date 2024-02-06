using System.Security.Cryptography;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;

using TeamUp.Application.Users;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Infrastructure.Options;

namespace TeamUp.Infrastructure.Security;

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
		var hash = HashPassword(salt, password, _authentication.Value.Pepper);
		return new Password(salt, hash);
	}

	public bool VerifyPassword(string inputRawPassword, Password dbPassword)
	{
		var hash = HashPassword(dbPassword.Salt, inputRawPassword, _authentication.Value.Pepper);
		return dbPassword.Hash.SequenceEqual(hash);
	}

	private byte[] HashPassword(byte[] salt, string password, string pepper)
	{
		return KeyDerivation.Pbkdf2(
			password: string.Concat(password, pepper),
			salt: salt,
			prf: KeyDerivationPrf.HMACSHA512,
			iterationCount: _authentication.Value.Pbkdf2Iterations,
			numBytesRequested: Password.HASH_SIZE
		);
	}
}
