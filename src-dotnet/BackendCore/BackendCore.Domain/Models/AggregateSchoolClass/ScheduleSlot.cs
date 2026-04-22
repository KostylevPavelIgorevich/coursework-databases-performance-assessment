using BackendCore.BackendCore.Domain.Models.Base;

namespace BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;

public class ScheduleSlot : Entity
{
    public Guid SchoolClassId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public int LessonNumber { get; private set; }
    public Guid TeachingAssignmentId { get; private set; }
    public Guid ClassroomId { get; private set; }

    public ScheduleSlot(
        Guid schoolClassId,
        DayOfWeek dayOfWeek,
        int lessonNumber,
        Guid teachingAssignmentId,
        Guid classroomId
    )
    {
        if (schoolClassId == Guid.Empty)
        {
            throw new ArgumentException("Класс обязателен.", nameof(schoolClassId));
        }

        if (dayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dayOfWeek),
                "Школьное расписание поддерживает только будние дни."
            );
        }

        if (lessonNumber is < 1 or > 10)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lessonNumber),
                "Номер урока должен быть в диапазоне 1..10."
            );
        }

        if (teachingAssignmentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Назначение учителя обязательно.",
                nameof(teachingAssignmentId)
            );
        }

        if (classroomId == Guid.Empty)
        {
            throw new ArgumentException("Кабинет обязателен.", nameof(classroomId));
        }

        SchoolClassId = schoolClassId;
        DayOfWeek = dayOfWeek;
        LessonNumber = lessonNumber;
        TeachingAssignmentId = teachingAssignmentId;
        ClassroomId = classroomId;
    }
}
