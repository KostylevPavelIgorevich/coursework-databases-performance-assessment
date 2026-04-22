using BackendCore.BackendCore.Domain.Models.Base;

namespace BackendCore.BackendCore.Domain.Models.AggregateLesson;

public class Grade : Entity
{
    public Guid LessonId { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid GradeTypeId { get; private set; }
    public int Value { get; private set; }
    public string? Comment { get; private set; }

    public Grade(Guid lessonId, Guid studentId, Guid gradeTypeId, int value, string? comment = null)
    {
        if (lessonId == Guid.Empty)
        {
            throw new ArgumentException("Урок обязателен.", nameof(lessonId));
        }

        if (studentId == Guid.Empty)
        {
            throw new ArgumentException("Ученик обязателен.", nameof(studentId));
        }

        if (gradeTypeId == Guid.Empty)
        {
            throw new ArgumentException("Тип оценки обязателен.", nameof(gradeTypeId));
        }

        if (value is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Оценка должна быть в диапазоне 1..5."
            );
        }

        LessonId = lessonId;
        StudentId = studentId;
        GradeTypeId = gradeTypeId;
        Value = value;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
    }

    public void ChangeValue(int value)
    {
        if (value is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Оценка должна быть в диапазоне 1..5."
            );
        }

        Value = value;
    }
}
