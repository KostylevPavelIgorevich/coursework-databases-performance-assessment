using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Domain.Models.AggregateStudent;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Repositories;

public sealed class EnrollmentRepository : IEnrollmentRepository
{
    private readonly SchoolDbContext _dbContext;

    public EnrollmentRepository(SchoolDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Enrollment entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Enrollments.AddAsync(entity, cancellationToken);
    }

    public Task<bool> HasOverlappingEnrollmentAsync(
        int studentId,
        DateOnly startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default
    )
    {
        var rangeEnd = endDate ?? DateOnly.MaxValue;

        return _dbContext.Enrollments.AnyAsync(
            x => x.StudentId == studentId
                && x.StartDate <= rangeEnd
                && (x.EndDate ?? DateOnly.MaxValue) >= startDate,
            cancellationToken
        );
    }
}
