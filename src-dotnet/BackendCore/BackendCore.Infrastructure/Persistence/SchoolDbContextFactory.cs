using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BackendCore.BackendCore.Infrastructure.Persistence;

public class SchoolDbContextFactory : IDesignTimeDbContextFactory<SchoolDbContext>
{
    public SchoolDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SchoolDbContext>();

        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__SchoolDb")
            ?? "Host=localhost;Port=5432;Database=school_db;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);
        return new SchoolDbContext(optionsBuilder.Options);
    }
}
