using System.Linq.Expressions;
using System.Reflection;

namespace TeamUp.EndToEndTests.Extensions;

public sealed class PrivateBinder : Bogus.Binder
{
	private readonly string[] _fields;

	public PrivateBinder(params string[] fields)
	{
		_fields = fields;
	}

	public override Dictionary<string, MemberInfo> GetMembers(Type type)
	{
		var members = base.GetMembers(type);

		var privateFields = type
			.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
			.Where(field => _fields.Contains(field.Name));

		foreach (var field in privateFields)
		{
			members.Add(field.Name, field);
		}

		return members;
	}
}

public static class FakerExtensions
{
	public static readonly Faker F = new();

	public static Faker<T> UsePrivateConstructor<T>(this Faker<T> faker) where T : class
		=> faker.CustomInstantiator(f => (Activator.CreateInstance(typeof(T), nonPublic: true) as T)!);

	public static Faker<T> RuleForBackingField<T, TProperty>(this Faker<T> faker, Expression<Func<T, TProperty>> property, TProperty value) where T : class
	{
		if (property.Body is not MemberExpression memberExpression || memberExpression.Member is not PropertyInfo propertyInfo)
		{
			throw new ArgumentException("Expression must be a MemberExpression pointing to a PropertyInfo", nameof(property));
		}

		var backingField = propertyInfo.Name.GetBackingField();
		return faker.RuleFor(backingField, _ => value);
	}

	public static string GetBackingField(this string propertyName) => $"<{propertyName}>k__BackingField";
}
