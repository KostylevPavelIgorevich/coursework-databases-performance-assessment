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
            ?? "Data Source=../BackendRunner/data/school.db";

        optionsBuilder.UseSqlite(connectionString);
        return new SchoolDbContext(optionsBuilder.Options);
    }
}
