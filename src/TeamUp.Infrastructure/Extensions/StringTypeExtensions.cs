using System.Reflection;

namespace TeamUp.Infrastructure.Extensions;

public static class StringTypeExtensions
{
	public static Type? ResolveType(this string type)
	{
		//{TeamUp.Assembly}.Namespace.X.Y.Z...

		var dot1 = type.IndexOf('.') + 1;
		var dot2 = type.IndexOf('.', dot1);

		var assemblyName = type[..dot2];
		var assembly = Assembly.Load(assemblyName);

		return assembly.GetType(type);
	}
}
