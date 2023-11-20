using TeamUp.Domain.SeedWork;

namespace TeamUp.Domain.Aggregates.Users;

public sealed record UserId : TypedId<UserId>;

public sealed class User : AggregateRoot<User, UserId>
{
	public string Name { get; private set; }
	public string Email { get; private set; }
	public Password Password { get; private set; }
	public UserStatus Status { get; private set; }

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
		Status = UserStatus.Active;
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
