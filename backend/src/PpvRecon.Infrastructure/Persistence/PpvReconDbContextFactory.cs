using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PpvRecon.Infrastructure.Persistence;

public sealed class PpvReconDbContextFactory : IDesignTimeDbContextFactory<PpvReconDbContext>
{
    public PpvReconDbContext CreateDbContext(string[] args)
    {
        var apiProjectPath = FindApiProjectPath();
        var appDataPath = Path.Combine(apiProjectPath, "App_Data");
        Directory.CreateDirectory(appDataPath);

        var dbPath = Path.Combine(appDataPath, "ppv-recon.db");
        var optionsBuilder = new DbContextOptionsBuilder<PpvReconDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new PpvReconDbContext(optionsBuilder.Options);
    }

    private static string FindApiProjectPath()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (current is not null)
        {
            var direct = Path.Combine(current.FullName, "src", "PpvRecon.Api");
            if (File.Exists(Path.Combine(direct, "PpvRecon.Api.csproj")))
            {
                return direct;
            }

            var sibling = Path.Combine(current.FullName, "PpvRecon.Api");
            if (File.Exists(Path.Combine(sibling, "PpvRecon.Api.csproj")))
            {
                return sibling;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Khong tim thay thu muc PpvRecon.Api de tao SQLite database.");
    }
}
