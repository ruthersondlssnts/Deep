using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Deep.Common.Application.Auditing;

public static class AuditingExtensions
{
    public static IServiceCollection AddAuditing(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddScoped<IAuditingUserProvider, JwtAuditingUserProvider>();
        services.TryAddScoped<WriteAuditLogInterceptor>();
        return services;
    }
}
