using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;

namespace BackendCore.BackendCore.Application.Abstractions.Persistence;

public interface IScheduleSlotRepository
{
    Task<ScheduleSlot?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsForClassAsync(
        int schoolClassId,
        DayOfWeek dayOfWeek,
        int lessonNumber,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(ScheduleSlot entity, CancellationToken cancellationToken = default);
}
