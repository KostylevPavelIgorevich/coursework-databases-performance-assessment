using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Application.Abstractions.UseCases;
using BackendCore.BackendCore.Application.Contracts.Common;
using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;

namespace BackendCore.BackendCore.Application.UseCases.ScheduleSlots.CreateScheduleSlot;

public sealed class CreateScheduleSlotHandler
    : IUseCase<CreateScheduleSlotCommand, OperationResult<int>>
{
    private readonly IScheduleSlotRepository _scheduleSlotRepository;
    private readonly ISchoolClassRepository _schoolClassRepository;
    private readonly ITeachingAssignmentRepository _teachingAssignmentRepository;
    private readonly IClassroomRepository _classroomRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateScheduleSlotHandler(
        IScheduleSlotRepository scheduleSlotRepository,
        ISchoolClassRepository schoolClassRepository,
        ITeachingAssignmentRepository teachingAssignmentRepository,
        IClassroomRepository classroomRepository,
        IUnitOfWork unitOfWork
    )
    {
        _scheduleSlotRepository = scheduleSlotRepository;
        _schoolClassRepository = schoolClassRepository;
        _teachingAssignmentRepository = teachingAssignmentRepository;
        _classroomRepository = classroomRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<int>> ExecuteAsync(
        CreateScheduleSlotCommand request,
        CancellationToken cancellationToken = default
    )
    {
        if (!await _schoolClassRepository.ExistsAsync(request.SchoolClassId, cancellationToken))
        {
            return OperationResult<int>.Failure("Класс не найден.");
        }

        if (
            !await _teachingAssignmentRepository.ExistsAsync(
                request.TeachingAssignmentId,
                cancellationToken
            )
        )
        {
            return OperationResult<int>.Failure("Назначение учителя не найдено.");
        }

        if (!await _classroomRepository.ExistsAsync(request.ClassroomId, cancellationToken))
        {
            return OperationResult<int>.Failure("Кабинет не найден.");
        }

        if (
            await _scheduleSlotRepository.ExistsForClassAsync(
                request.SchoolClassId,
                request.DayOfWeek,
                request.LessonNumber,
                cancellationToken
            )
        )
        {
            return OperationResult<int>.Failure("Такой слот расписания для класса уже существует.");
        }

        ScheduleSlot scheduleSlot;
        try
        {
            scheduleSlot = new ScheduleSlot(
                request.SchoolClassId,
                request.DayOfWeek,
                request.LessonNumber,
                request.TeachingAssignmentId,
                request.ClassroomId
            );
        }
        catch (ArgumentException ex)
        {
            return OperationResult<int>.Failure(ex.Message);
        }

        await _scheduleSlotRepository.AddAsync(scheduleSlot, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Success(scheduleSlot.Id);
    }
}
