using Microsoft.Extensions.Configuration;

namespace Deep.Api.Extensions;

internal static class ConfigurationExtensions
{
    internal static void AddModuleConfiguration(
        this IConfigurationBuilder configurationBuilder,
        IEnumerable<string> modules
    )
    {
        foreach (string module in modules)
        {
            configurationBuilder.AddJsonFile($"modules.{module}.json", false, true);
            configurationBuilder.AddJsonFile(
                $"modules.{module}.Development.json",
                optional: true,
                reloadOnChange: true
            );
        }
    }
}
