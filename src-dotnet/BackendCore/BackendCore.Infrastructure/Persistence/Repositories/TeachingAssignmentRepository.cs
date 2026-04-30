using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Repositories;

public sealed class TeachingAssignmentRepository : ITeachingAssignmentRepository
{
    private readonly SchoolDbContext _dbContext;

    public TeachingAssignmentRepository(SchoolDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TeachingAssignment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.TeachingAssignments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.TeachingAssignments.AnyAsync(x => x.Id == id, cancellationToken);
    }
}
