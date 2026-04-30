using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;

namespace BackendCore.BackendCore.Application.Abstractions.Persistence;

public interface ITeachingAssignmentRepository
{
    Task<TeachingAssignment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
