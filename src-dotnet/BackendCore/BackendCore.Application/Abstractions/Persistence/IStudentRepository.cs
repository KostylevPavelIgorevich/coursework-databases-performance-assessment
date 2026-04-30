using BackendCore.BackendCore.Domain.Models.AggregateStudent;

namespace BackendCore.BackendCore.Application.Abstractions.Persistence;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Student entity, CancellationToken cancellationToken = default);
}
