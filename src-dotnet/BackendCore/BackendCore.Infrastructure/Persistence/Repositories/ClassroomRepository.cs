using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Repositories;

public sealed class ClassroomRepository : IClassroomRepository
{
    private readonly SchoolDbContext _dbContext;

    public ClassroomRepository(SchoolDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Classroom?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Classrooms.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Classrooms.AnyAsync(x => x.Id == id, cancellationToken);
    }
}
