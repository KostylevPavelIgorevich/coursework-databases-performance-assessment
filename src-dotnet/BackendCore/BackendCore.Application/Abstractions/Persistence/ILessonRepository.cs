using BackendCore.BackendCore.Domain.Models.AggregateLesson;

namespace BackendCore.BackendCore.Application.Abstractions.Persistence;

public interface ILessonRepository
{
    Task<Lesson?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlotAndDateAsync(
        int scheduleSlotId,
        DateOnly date,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(Lesson entity, CancellationToken cancellationToken = default);
}
