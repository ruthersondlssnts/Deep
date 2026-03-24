using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Vast.Common.Application.SimpleMediatR;

public static class RequestHandlerRegistrationExtensions
{
    public static IServiceCollection AddRequestHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        services.Scan(scan =>
            scan.FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        return services;
    }
}
