namespace BackendCore.BackendCore.Application.UseCases.Enrollments.EnrollStudent;

public sealed record EnrollStudentCommand(
    int StudentId,
    int SchoolClassId,
    int AcademicYearId,
    DateOnly StartDate,
    DateOnly? EndDate = null
);
