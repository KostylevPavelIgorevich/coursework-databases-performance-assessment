namespace BackendCore.BackendCore.Application.UseCases.Grades.AssignGrade;

public sealed record AssignGradeCommand(
    int LessonId,
    int StudentId,
    int GradeTypeId,
    int Value,
    string? Comment = null
);
