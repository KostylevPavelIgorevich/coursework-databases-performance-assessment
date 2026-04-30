namespace BackendCore.BackendCore.Application.UseCases.Students.CreateStudent;

public sealed record CreateStudentCommand(
    string LastName,
    string FirstName,
    string MiddleName,
    DateOnly BirthDate,
    int StudentStatusId
);
