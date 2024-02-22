using TeamUp.Contracts.Users;
using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Users;

public sealed class User : AggregateRoot<User, UserId>
{
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

	public override string ToString() => Name;

	public void Activate()
	{
		Status = UserStatus.Activated;
		AddDomainEvent(new UserActivatedDomainEvent(this));
	}

	internal static User Generate(string email) => new(
		UserId.New(),
		email,
		email,
		new Password(),
		UserStatus.Generated
	);

	internal static User Create(string name, string email, Password password) => new(
		UserId.New(),
		name,
		email,
		password,
		UserStatus.NotActivated
	);
}
