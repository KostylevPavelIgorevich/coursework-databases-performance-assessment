namespace BackendCore.BackendCore.API.Contracts;

public sealed record CreateStudentRequest(
    string LastName,
    string FirstName,
    string MiddleName,
    DateOnly BirthDate,
    int StudentStatusId,
    int ClassId
);

public sealed record UpdateStudentRequest(
    string LastName,
    string FirstName,
    string MiddleName,
    DateOnly BirthDate
);

public sealed record CreateClassRequest(string Title);
public sealed record UpdateClassRequest(string Title);

public sealed record CreateSubjectRequest(string Name, string? ShortName = null);
public sealed record UpdateSubjectRequest(string Name, string? ShortName = null);

public sealed record CreateTeacherRequest(string LastName, string FirstName, string MiddleName);
public sealed record UpdateTeacherRequest(string LastName, string FirstName, string MiddleName);

public sealed record CreateScheduleRequest(
    int ClassId,
    string Day,
    int LessonNumber,
    int SubjectId,
    int TeacherId
);

public sealed record UpdateScheduleRequest(
    int ClassId,
    string Day,
    int LessonNumber,
    int SubjectId,
    int TeacherId
);

public sealed record CreateGradeRequest(int StudentId, int SubjectId, DateOnly Date, int Value);
public sealed record UpdateGradeRequest(int Value, string? Comment = null);
