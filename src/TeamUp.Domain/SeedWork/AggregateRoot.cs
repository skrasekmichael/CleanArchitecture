﻿using System.Linq.Expressions;
using System.Reflection;

using TeamUp.Common;

namespace TeamUp.Domain.SeedWork;

public abstract class AggregateRoot<TSelf, TId> : Entity<TId>
	where TSelf : AggregateRoot<TSelf, TId>
	where TId : TypedId<TId>, new()
{
	protected AggregateRoot(TId id) : base(id) { }

	protected bool UpdateProperty<TProperty>(Expression<Func<TSelf, TProperty>> selector, TProperty newValue, IDomainEvent? domainEvent = null)
		where TProperty : IEquatable<TProperty>
	{
		var member = (selector.Body as MemberExpression)?.Member;
		if (member is not PropertyInfo propertyInfo)
			throw new InternalException($"Invalid selector. [{selector}]");

		var value = propertyInfo.GetValue(this);
		if (newValue.Equals(value))
			return false;

		propertyInfo.SetValue(this, newValue);
		if (domainEvent is not null)
			AddDomainEvent(domainEvent);

		return true;
	}
}
