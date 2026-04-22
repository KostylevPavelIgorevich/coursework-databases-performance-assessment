using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Application.Abstractions.UseCases;
using BackendCore.BackendCore.Application.Contracts.Common;
using BackendCore.BackendCore.Domain.Models.AggregateLesson;

namespace BackendCore.BackendCore.Application.UseCases.Grades.AssignGrade;

public sealed class AssignGradeHandler : IUseCase<AssignGradeCommand, OperationResult<int>>
{
    private readonly IGradeRepository _gradeRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IGradeTypeRepository _gradeTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignGradeHandler(
        IGradeRepository gradeRepository,
        ILessonRepository lessonRepository,
        IStudentRepository studentRepository,
        IGradeTypeRepository gradeTypeRepository,
        IUnitOfWork unitOfWork
    )
    {
        _gradeRepository = gradeRepository;
        _lessonRepository = lessonRepository;
        _studentRepository = studentRepository;
        _gradeTypeRepository = gradeTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<int>> ExecuteAsync(
        AssignGradeCommand request,
        CancellationToken cancellationToken = default
    )
    {
        if (!await _lessonRepository.ExistsAsync(request.LessonId, cancellationToken))
        {
            return OperationResult<int>.Failure("Урок не найден.");
        }

        if (!await _studentRepository.ExistsAsync(request.StudentId, cancellationToken))
        {
            return OperationResult<int>.Failure("Ученик не найден.");
        }

        if (!await _gradeTypeRepository.ExistsAsync(request.GradeTypeId, cancellationToken))
        {
            return OperationResult<int>.Failure("Тип оценки не найден.");
        }

        if (
            await _gradeRepository.ExistsAsync(
                request.LessonId,
                request.StudentId,
                request.GradeTypeId,
                cancellationToken
            )
        )
        {
            return OperationResult<int>.Failure(
                "Оценка такого типа для ученика на этом уроке уже выставлена."
            );
        }

        Grade grade;
        try
        {
            grade = new Grade(
                request.LessonId,
                request.StudentId,
                request.GradeTypeId,
                request.Value,
                request.Comment
            );
        }
        catch (ArgumentException ex)
        {
            return OperationResult<int>.Failure(ex.Message);
        }

        await _gradeRepository.AddAsync(grade, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Success(grade.Id);
    }
}
