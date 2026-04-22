using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;

namespace BackendCore.BackendCore.Application.Abstractions.Persistence;

public interface ISchoolClassRepository
{
    Task<SchoolClass?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
