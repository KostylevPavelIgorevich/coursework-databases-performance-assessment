using BackendCore.BackendCore.Application.DependencyInjection;
using BackendCore.BackendCore.Application.Abstractions.UseCases;
using BackendCore.BackendCore.Application.Contracts.Common;
using BackendCore.BackendCore.Application.UseCases.Students.CreateStudent;
using BackendCore.BackendCore.Infrastructure.Persistence;
using BackendCore.BackendCore.Infrastructure.DependencyInjection;
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
services.AddApplication();
services.AddInfrastructure(connectionString);

using var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();
await SchoolDbInitializer.InitializeAsync(dbContext);

var createStudentUseCase = scope.ServiceProvider.GetRequiredService<
    IUseCase<CreateStudentCommand, OperationResult<int>>
>();
_ = createStudentUseCase;

Console.WriteLine("База данных проверена, миграции применены, базовый сидинг выполнен.");
