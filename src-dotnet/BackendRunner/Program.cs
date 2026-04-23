using BackendCore.BackendCore.API.DependencyInjection;
using BackendCore.BackendCore.Application.DependencyInjection;
using BackendCore.BackendCore.Infrastructure.DependencyInjection;
using BackendCore.BackendCore.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("SchoolDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Строка подключения 'ConnectionStrings:SchoolDb' не задана.");
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "frontend",
        policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
    );
});

var app = builder.Build();
app.UseCors("frontend");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();
    await SchoolDbInitializer.InitializeAsync(dbContext);
}

app.MapSchoolApi();
app.Run("http://localhost:5050");
