using BackendCore.BackendCore.Domain.Models.AggregateLesson;

namespace BackendCore.BackendCore.Application.Abstractions.Persistence;

public interface IGradeRepository
{
    Task<bool> ExistsAsync(
        int lessonId,
        int studentId,
        int gradeTypeId,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(Grade entity, CancellationToken cancellationToken = default);
}
