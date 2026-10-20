using Microsoft.Extensions.Configuration;

namespace EFCoreVsDapper;
public static class AppConfiguration
{
    // Environment variable ConnectionStrings__Default overrides appsettings.
    // The environment defaults to Development (this is a local tool); set DOTNET_ENVIRONMENT to change it.
    public static string GetConnectionString()
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        return configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' is not configured. Set ConnectionStrings__Default or add it to appsettings.{Environment}.json.");
    }
}
