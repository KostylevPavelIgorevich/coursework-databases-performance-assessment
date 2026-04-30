using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Domain.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Repositories;

public sealed class AcademicYearRepository : IAcademicYearRepository
{
    private readonly SchoolDbContext _dbContext;

    public AcademicYearRepository(SchoolDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AcademicYear?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.AcademicYears.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.AcademicYears.AnyAsync(x => x.Id == id, cancellationToken);
    }
}
