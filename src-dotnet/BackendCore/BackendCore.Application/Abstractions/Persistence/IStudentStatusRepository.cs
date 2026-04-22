using BackendCore.BackendCore.Domain.Models.AggregateStudent;

namespace BackendCore.BackendCore.Application.Abstractions.Persistence;

public interface IStudentStatusRepository
{
    Task<StudentStatus?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
