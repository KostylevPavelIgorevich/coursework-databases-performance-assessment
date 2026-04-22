using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Application.Abstractions.UseCases;
using BackendCore.BackendCore.Application.Contracts.Common;
using BackendCore.BackendCore.Domain.Models.AggregateStudent;

namespace BackendCore.BackendCore.Application.UseCases.Students.CreateStudent;

public sealed class CreateStudentHandler : IUseCase<CreateStudentCommand, OperationResult<int>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentStatusRepository _studentStatusRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStudentHandler(
        IStudentRepository studentRepository,
        IStudentStatusRepository studentStatusRepository,
        IUnitOfWork unitOfWork
    )
    {
        _studentRepository = studentRepository;
        _studentStatusRepository = studentStatusRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<int>> ExecuteAsync(
        CreateStudentCommand request,
        CancellationToken cancellationToken = default
    )
    {
        if (!await _studentStatusRepository.ExistsAsync(request.StudentStatusId, cancellationToken))
        {
            return OperationResult<int>.Failure("Указанный статус ученика не найден.");
        }

        Student student;
        try
        {
            student = new Student(
                request.LastName,
                request.FirstName,
                request.MiddleName,
                request.BirthDate,
                request.StudentStatusId
            );
        }
        catch (ArgumentException ex)
        {
            return OperationResult<int>.Failure(ex.Message);
        }

        await _studentRepository.AddAsync(student, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Success(student.Id);
    }
}
