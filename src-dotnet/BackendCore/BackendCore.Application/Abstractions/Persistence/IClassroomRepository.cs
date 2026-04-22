using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;

namespace BackendCore.BackendCore.Application.Abstractions.Persistence;

public interface IClassroomRepository
{
    Task<Classroom?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
