using BackendCore.BackendCore.Domain.Models.Common;

namespace BackendCore.BackendCore.Application.Abstractions.Persistence;

public interface IAcademicYearRepository
{
    Task<AcademicYear?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
