using BackendCore.BackendCore.Domain.Models.Base;

namespace BackendCore.BackendCore.Domain.Models.AggregateLesson;

public class Grade : Entity
{
    public int LessonId { get; private set; }
    public int StudentId { get; private set; }
    public int GradeTypeId { get; private set; }
    public int Value { get; private set; }
    public string? Comment { get; private set; }

    public Grade(int lessonId, int studentId, int gradeTypeId, int value, string? comment = null)
    {
        if (lessonId <= 0)
        {
            throw new ArgumentException("Урок обязателен.", nameof(lessonId));
        }

        if (studentId <= 0)
        {
            throw new ArgumentException("Ученик обязателен.", nameof(studentId));
        }

        if (gradeTypeId <= 0)
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

    public void ChangeGradeType(int gradeTypeId)
    {
        if (gradeTypeId <= 0)
        {
            throw new ArgumentException("Тип оценки обязателен.", nameof(gradeTypeId));
        }

        GradeTypeId = gradeTypeId;
    }

    public void ChangeComment(string? comment)
    {
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
    }
}
