using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Application.Abstractions.UseCases;
using BackendCore.BackendCore.Application.Contracts.Common;
using BackendCore.BackendCore.Domain.Models.AggregateLesson;

namespace BackendCore.BackendCore.Application.UseCases.Lessons.CreateLesson;

public sealed class CreateLessonHandler : IUseCase<CreateLessonCommand, OperationResult<int>>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IScheduleSlotRepository _scheduleSlotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLessonHandler(
        ILessonRepository lessonRepository,
        IScheduleSlotRepository scheduleSlotRepository,
        IUnitOfWork unitOfWork
    )
    {
        _lessonRepository = lessonRepository;
        _scheduleSlotRepository = scheduleSlotRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<int>> ExecuteAsync(
        CreateLessonCommand request,
        CancellationToken cancellationToken = default
    )
    {
        if (!await _scheduleSlotRepository.ExistsAsync(request.ScheduleSlotId, cancellationToken))
        {
            return OperationResult<int>.Failure("Слот расписания не найден.");
        }

        if (
            await _lessonRepository.ExistsBySlotAndDateAsync(
                request.ScheduleSlotId,
                request.Date,
                cancellationToken
            )
        )
        {
            return OperationResult<int>.Failure("Урок для этого слота и даты уже существует.");
        }

        Lesson lesson;
        try
        {
            lesson = new Lesson(request.ScheduleSlotId, request.Date, request.Topic);
        }
        catch (ArgumentException ex)
        {
            return OperationResult<int>.Failure(ex.Message);
        }

        await _lessonRepository.AddAsync(lesson, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Success(lesson.Id);
    }
}
