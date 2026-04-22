using BackendCore.BackendCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetConnectionString("SchoolDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Строка подключения 'ConnectionStrings:SchoolDb' не задана."
    );
}

var services = new ServiceCollection();
services.AddDbContext<SchoolDbContext>(options => options.UseNpgsql(connectionString));

using var serviceProvider = services.BuildServiceProvider();
_ = serviceProvider.GetRequiredService<SchoolDbContext>();

Console.WriteLine("SchoolDbContext успешно настроен.");
