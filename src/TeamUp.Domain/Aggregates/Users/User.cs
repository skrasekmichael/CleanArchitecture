using System.Linq.Expressions;

using TeamUp.Contracts.Users;
using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Users;

public sealed class User : AggregateRoot<User, UserId>
{
	internal static readonly TimeSpan GeneratedUserTTL = TimeSpan.FromDays(2);
	internal static readonly TimeSpan RegisteredUserTTL = TimeSpan.FromDays(1);

	public string Name { get; private set; }
	public string Email { get; private set; }
	public Password Password { get; private set; }
	public UserStatus Status { get; private set; }
	public int NumberOfOwnedTeams { get; private set; }
	public DateTime CreatedUtc { get; private set; }

#pragma warning disable CS8618 // EF Core constructor
	private User() : base() { }
#pragma warning restore CS8618

	internal User(UserId id, string name, string email, Password password, UserStatus status, DateTime utcNow) : base(id)
	{
		Name = name;
		Email = email;
		Password = password;
		Status = status;
		CreatedUtc = utcNow;

		AddDomainEvent(new UserCreatedDomainEvent(this));
	}

	public override string ToString() => $"{Name} ({Status})";

	public void Activate()
	{
		Status = UserStatus.Activated;
		AddDomainEvent(new UserActivatedDomainEvent(this));
	}

	public void Delete()
	{
		AddDomainEvent(new UserDeletedDomainEvent(this));
	}

	internal void IncreaseNumberOfOwningTeams() => NumberOfOwnedTeams += 1;

	internal void DecreaseNumberOfOwningTeams() => NumberOfOwnedTeams -= 1;

	internal static Expression<Func<User, bool>> AccountHasExpiredExpression(DateTime utcNow)
	{
		return user =>
			(user.Status == UserStatus.Generated && (utcNow - user.CreatedUtc) > GeneratedUserTTL) ||
			(user.Status == UserStatus.NotActivated && (utcNow - user.CreatedUtc) > RegisteredUserTTL);
	}
}
