using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using PpvRecon.Application.Auth;
using PpvRecon.Infrastructure.Persistence;
using PpvRecon.Infrastructure.Security;

namespace PpvRecon.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=App_Data/ppv-recon.db";

        connectionString = ResolveSqliteConnectionString(connectionString, contentRootPath);

        services.AddDbContext<PpvReconDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

        return services;
    }

    private static string ResolveSqliteConnectionString(string connectionString, string contentRootPath)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);

        if (!string.IsNullOrWhiteSpace(builder.DataSource)
            && builder.DataSource != ":memory:"
            && !Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.GetFullPath(Path.Combine(contentRootPath, builder.DataSource));
        }

        var directory = Path.GetDirectoryName(builder.DataSource);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return builder.ToString();
    }
}
