using Microsoft.Extensions.Configuration;

namespace Dota2Notify.Api;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Gets a configuration value from environment variables first, then falls back to configuration
    /// </summary>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="key">Configuration key (e.g. "CosmosDb:EndpointUri")</param>
    /// <returns>The configuration value or null if not found</returns>
    public static string? GetValueWithEnvOverride(this IConfiguration configuration, string key)
    {
        // Convert "CosmosDb:EndpointUri" to "COSMOSDB__ENDPOINTURI" for environment variables
        var envKey = key.Replace(":", "__").ToUpper();
        
        // Try to get value from environment variable first
        var envValue = Environment.GetEnvironmentVariable(envKey);
        
        // If environment variable exists, return it, otherwise fallback to configuration
        return !string.IsNullOrEmpty(envValue) ? envValue : configuration[key];
    }
}
