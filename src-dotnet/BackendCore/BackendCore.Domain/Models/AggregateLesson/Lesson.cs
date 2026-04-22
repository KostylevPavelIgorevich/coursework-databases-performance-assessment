using BackendCore.BackendCore.Domain.Models.Base;

namespace BackendCore.BackendCore.Domain.Models.AggregateLesson;

public class Lesson : Entity
{
    public Guid ScheduleSlotId { get; private set; }
    public DateOnly Date { get; private set; }
    public string Topic { get; private set; }

    public Lesson(Guid scheduleSlotId, DateOnly date, string topic)
    {
        if (scheduleSlotId == Guid.Empty)
        {
            throw new ArgumentException("Слот расписания обязателен.", nameof(scheduleSlotId));
        }

        if (date > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentOutOfRangeException(
                nameof(date),
                "Дата урока не может быть в будущем."
            );
        }

        if (string.IsNullOrWhiteSpace(topic))
        {
            throw new ArgumentException("Тема урока обязательна.", nameof(topic));
        }

        ScheduleSlotId = scheduleSlotId;
        Date = date;
        Topic = topic.Trim();
    }
}
