namespace BackendCore.BackendCore.Application.Dtos.Students;

public sealed record StudentDto(
    int Id,
    string LastName,
    string FirstName,
    string MiddleName,
    DateOnly BirthDate,
    int StudentStatusId
);
