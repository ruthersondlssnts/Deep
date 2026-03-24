using Microsoft.Extensions.DependencyInjection;

namespace Vast.Common.Application.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationInternal(this IServiceCollection services)
    {
        services.AddAuthentication().AddJwtBearer();

        services.AddHttpContextAccessor();

        services.ConfigureOptions<JwtBearerConfigureOptions>();

        return services;
    }
}
