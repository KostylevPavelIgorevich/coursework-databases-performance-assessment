using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Domain.Models.AggregateLesson;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Repositories;

public sealed class GradeTypeRepository : IGradeTypeRepository
{
    private readonly SchoolDbContext _dbContext;

    public GradeTypeRepository(SchoolDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<GradeType?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.GradeTypes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.GradeTypes.AnyAsync(x => x.Id == id, cancellationToken);
    }
}
