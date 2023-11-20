namespace TeamUp.Domain.Aggregates.Users;

public sealed class Password : IEquatable<Password>
{
	public const int SaltSize = 16; //128 bit salt
	public const int HashSize = 64; //512 bit hash
	public const int TotalSize = SaltSize + HashSize;

	public byte[] Salt { get; private set; }
	public byte[] Hash { get; private set; }

	public Password()
	{
		Salt = new byte[SaltSize];
		Hash = new byte[HashSize];
	}

	public Password(byte[] salt, byte[] hash)
	{
		if (salt.Length != SaltSize)
			throw new ArgumentException($"Received {salt.Length} bytes, {SaltSize} was expected.", nameof(salt));
		else if (hash.Length != HashSize)
			throw new ArgumentException($"Received {hash.Length} bytes, {HashSize} was expected.", nameof(hash));

		Salt = salt;
		Hash = hash;
	}

	public Password(byte[] bytes)
	{
		if (bytes.Length != TotalSize)
			throw new ArgumentException($"Received {bytes.Length} bytes, {TotalSize} was expected.", nameof(bytes));

		Salt = bytes[..SaltSize];
		Hash = bytes[SaltSize..];
	}

	public byte[] GetBytes()
	{
		var bytes = new byte[TotalSize];
		Salt.CopyTo(bytes, 0);
		Hash.CopyTo(bytes, SaltSize);
		return bytes;
	}

	public bool Equals(Password? other) => other is Password password && this == password;

	public override bool Equals(object? obj) => obj is Password password && this == password;

	public override int GetHashCode() => HashCode.Combine(Salt, Hash);

	public static bool operator ==(Password left, Password right) =>
		left.Salt.SequenceEqual(right.Salt) &&
		left.Hash.SequenceEqual(right.Hash);

	public static bool operator !=(Password left, Password right) =>
		!left.Salt.SequenceEqual(right.Salt) ||
		!left.Hash.SequenceEqual(right.Hash);

	public override string ToString() => $"{Convert.ToHexString(Salt)}|{Convert.ToHexString(Hash)}";
}
