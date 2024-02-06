using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Users;

public sealed record UserId : TypedId<UserId>;

public sealed class User : AggregateRoot<User, UserId>
{
	public const int NAME_MAX_SIZE = 30;
	public const int NAME_MIN_SIZE = 3;

	public string Name { get; private set; }
	public string Email { get; private set; }
	public Password Password { get; private set; }
	public UserStatus Status { get; private set; }

#pragma warning disable CS8618 // EF Core constructor
	private User() : base() { }
#pragma warning restore CS8618

	private User(UserId id, string name, string email, Password password, UserStatus status) : base(id)
	{
		Name = name;
		Email = email;
		Password = password;
		Status = status;

		AddDomainEvent(new UserCreatedDomainEvent(this));
	}

	public void Activate()
	{
		Status = UserStatus.Activated;
		AddDomainEvent(new UserActivatedDomainEvent(this));
	}

	public static User Generate(string email) => new(
		UserId.New(),
		email,
		email,
		new Password(),
		UserStatus.Generated
	);

	public static User Create(string name, string email, Password password) => new(
		UserId.New(),
		name,
		email,
		password,
		UserStatus.NotActivated
	);
}
