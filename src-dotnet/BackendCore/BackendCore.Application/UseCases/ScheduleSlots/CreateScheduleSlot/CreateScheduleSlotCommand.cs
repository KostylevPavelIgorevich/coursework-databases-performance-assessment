namespace BackendCore.BackendCore.Application.UseCases.ScheduleSlots.CreateScheduleSlot;

public sealed record CreateScheduleSlotCommand(
    int SchoolClassId,
    DayOfWeek DayOfWeek,
    int LessonNumber,
    int TeachingAssignmentId,
    int ClassroomId
);
