using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Domain.Models.AggregateStudent;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Repositories;

public sealed class StudentStatusRepository : IStudentStatusRepository
{
    private readonly SchoolDbContext _dbContext;

    public StudentStatusRepository(SchoolDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<StudentStatus?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.StudentStatuses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.StudentStatuses.AnyAsync(x => x.Id == id, cancellationToken);
    }
}
