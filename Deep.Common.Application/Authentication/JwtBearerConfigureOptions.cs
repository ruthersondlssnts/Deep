using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Deep.Common.Application.Authentication;

internal sealed class JwtBearerConfigureOptions(IConfiguration configuration)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    private const string ConfigurationSectionName = "Authentication";

    public void Configure(JwtBearerOptions options) => Configure(Options.DefaultName, options);

    public void Configure(string? name, JwtBearerOptions options)
    {
        IConfigurationSection authSection = configuration.GetSection(ConfigurationSectionName);

        string? issuerSigningKey = authSection["TokenValidationParameters:IssuerSigningKey"];
        string? validIssuer = authSection["TokenValidationParameters:ValidIssuer"];
        string? validAudience = authSection["TokenValidationParameters:ValidAudience"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = validIssuer,
            ValidateAudience = true,
            ValidAudience = validAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey!)),
            ClockSkew = TimeSpan.FromMinutes(5),
        };

        // Don't use Authority for symmetric key validation - disable HTTPS metadata requirement
        options.RequireHttpsMetadata = false;
    }
}
