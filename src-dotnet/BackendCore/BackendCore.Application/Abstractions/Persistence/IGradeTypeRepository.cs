using BackendCore.BackendCore.Domain.Models.AggregateLesson;

namespace BackendCore.BackendCore.Application.Abstractions.Persistence;

public interface IGradeTypeRepository
{
    Task<GradeType?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
