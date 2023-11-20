using System.Linq.Expressions;

namespace TeamUp.Domain.SeedWork;

public abstract class AggregateRoot<TSelf, TId> : Entity<TId>
	where TSelf : AggregateRoot<TSelf, TId>
	where TId : TypedId<TId>, new()
{
	protected AggregateRoot(TId id) : base(id)
	{
	}

	protected bool UpdateProperty<TProperty>(Expression<Func<TSelf, TProperty>> selector, TProperty newValue, IDomainEvent? domainEvent = null)
		where TProperty : IEquatable<TProperty>
	{
		var propertyInfo = (selector.Body as MemberExpression)?.Member as System.Reflection.PropertyInfo;
		if (propertyInfo is null)
			throw new ArgumentException(nameof(selector), "Invalid property selector.");

		var value = propertyInfo.GetValue(this);
		if (value is null)
			throw new ArgumentException(nameof(selector), "Invalid property selector.");

		if (value.Equals(newValue))
			return false;

		propertyInfo.SetValue(this, newValue);
		if (domainEvent is not null)
			AddDomainEvent(domainEvent);

		return true;
	}
}
