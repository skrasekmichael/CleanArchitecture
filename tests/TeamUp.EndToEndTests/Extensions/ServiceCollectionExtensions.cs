using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TeamUp.EndToEndTests.Extensions;

public static class ServiceCollectionExtensions
{
	public static void Replace<TServiceType, TNewImplementationType>(this IServiceCollection services) where TNewImplementationType : class, TServiceType
	{
		var targetDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(TServiceType));
		if (targetDescriptor is not null)
		{
			services.Remove(targetDescriptor);
			services.Add(new ServiceDescriptor(
				typeof(TServiceType),
				typeof(TNewImplementationType),
				targetDescriptor.Lifetime)
			);
		}
	}
}
