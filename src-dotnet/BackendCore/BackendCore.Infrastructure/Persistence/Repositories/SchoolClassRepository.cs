using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Repositories;

public sealed class SchoolClassRepository : ISchoolClassRepository
{
    private readonly SchoolDbContext _dbContext;

    public SchoolClassRepository(SchoolDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SchoolClass?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.SchoolClasses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.SchoolClasses.AnyAsync(x => x.Id == id, cancellationToken);
    }
}
