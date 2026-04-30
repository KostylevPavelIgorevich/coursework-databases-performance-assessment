namespace BackendCore.BackendCore.Application.UseCases.Lessons.CreateLesson;

public sealed record CreateLessonCommand(int ScheduleSlotId, DateOnly Date, string Topic);
