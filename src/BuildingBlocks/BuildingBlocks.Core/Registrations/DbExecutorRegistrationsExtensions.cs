using System.Reflection;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Reflection.Extensions;

namespace BuildingBlocks.Core.Registrations;

public static class DbExecutorRegistrationsExtensions
{
    public static IServiceCollection ScanAndRegisterDbExecutors(
        this IServiceCollection services,
        params Assembly[] assembliesToScan
    )
    {
        var scanAssemblies = assembliesToScan.Any() ? assembliesToScan : new[] { Assembly.GetCallingAssembly(), };

        var dbExecutors = scanAssemblies
            .SelectMany(x => x.GetLoadableTypes())
            .Where(
                t =>
                    t!.IsClass
                    && !t.IsAbstract
                    && !t.IsGenericType
                    && !t.IsInterface
                    && t.GetConstructor(Type.EmptyTypes) != null
                    && typeof(IDbExecutors).IsAssignableFrom(t)
            )
            .ToList();

        foreach (var dbExecutor in dbExecutors)
        {
            var instantiatedType = (IDbExecutors)Activator.CreateInstance(dbExecutor)!;
            instantiatedType.Register(services);
        }

        return services;
    }
}
