using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Domain.Models.AggregateLesson;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Repositories;

public sealed class GradeRepository : IGradeRepository
{
    private readonly SchoolDbContext _dbContext;

    public GradeRepository(SchoolDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(
        int lessonId,
        int studentId,
        int gradeTypeId,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.Grades.AnyAsync(
            x => x.LessonId == lessonId && x.StudentId == studentId && x.GradeTypeId == gradeTypeId,
            cancellationToken
        );
    }

    public async Task AddAsync(Grade entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Grades.AddAsync(entity, cancellationToken);
    }
}
