using BackendCore.BackendCore.Domain.Models.AggregateStudent;

namespace BackendCore.BackendCore.Application.Abstractions.Persistence;

public interface IEnrollmentRepository
{
    Task AddAsync(Enrollment entity, CancellationToken cancellationToken = default);
    Task<bool> HasOverlappingEnrollmentAsync(
        int studentId,
        DateOnly startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default
    );
}
