using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Domain.Models.AggregateStudent;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Repositories;

public sealed class StudentRepository : IStudentRepository
{
    private readonly SchoolDbContext _dbContext;

    public StudentRepository(SchoolDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Students.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Students.AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Student entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Students.AddAsync(entity, cancellationToken);
    }
}
