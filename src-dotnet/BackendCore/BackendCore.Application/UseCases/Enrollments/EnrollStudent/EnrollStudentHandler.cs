using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Application.Abstractions.UseCases;
using BackendCore.BackendCore.Application.Contracts.Common;
using BackendCore.BackendCore.Domain.Models.AggregateStudent;

namespace BackendCore.BackendCore.Application.UseCases.Enrollments.EnrollStudent;

public sealed class EnrollStudentHandler : IUseCase<EnrollStudentCommand, OperationResult<int>>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolClassRepository _schoolClassRepository;
    private readonly IAcademicYearRepository _academicYearRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EnrollStudentHandler(
        IEnrollmentRepository enrollmentRepository,
        IStudentRepository studentRepository,
        ISchoolClassRepository schoolClassRepository,
        IAcademicYearRepository academicYearRepository,
        IUnitOfWork unitOfWork
    )
    {
        _enrollmentRepository = enrollmentRepository;
        _studentRepository = studentRepository;
        _schoolClassRepository = schoolClassRepository;
        _academicYearRepository = academicYearRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<int>> ExecuteAsync(
        EnrollStudentCommand request,
        CancellationToken cancellationToken = default
    )
    {
        if (!await _studentRepository.ExistsAsync(request.StudentId, cancellationToken))
        {
            return OperationResult<int>.Failure("Ученик не найден.");
        }

        if (!await _schoolClassRepository.ExistsAsync(request.SchoolClassId, cancellationToken))
        {
            return OperationResult<int>.Failure("Класс не найден.");
        }

        if (!await _academicYearRepository.ExistsAsync(request.AcademicYearId, cancellationToken))
        {
            return OperationResult<int>.Failure("Учебный год не найден.");
        }

        if (
            await _enrollmentRepository.HasOverlappingEnrollmentAsync(
                request.StudentId,
                request.StartDate,
                request.EndDate,
                cancellationToken
            )
        )
        {
            return OperationResult<int>.Failure(
                "Для ученика уже существует пересекающийся период зачисления."
            );
        }

        Enrollment enrollment;
        try
        {
            enrollment = new Enrollment(
                request.StudentId,
                request.SchoolClassId,
                request.AcademicYearId,
                request.StartDate,
                request.EndDate
            );
        }
        catch (ArgumentException ex)
        {
            return OperationResult<int>.Failure(ex.Message);
        }

        await _enrollmentRepository.AddAsync(enrollment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Success(enrollment.Id);
    }
}
