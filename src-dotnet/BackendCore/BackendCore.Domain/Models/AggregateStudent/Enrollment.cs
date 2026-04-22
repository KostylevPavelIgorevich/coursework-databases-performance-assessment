using BackendCore.BackendCore.Domain.Models.Base;

namespace BackendCore.BackendCore.Domain.Models.AggregateStudent;

public class Enrollment : Entity
{
    public Guid StudentId { get; private set; }
    public Guid SchoolClassId { get; private set; }
    public Guid AcademicYearId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }

    public Enrollment(
        Guid studentId,
        Guid schoolClassId,
        Guid academicYearId,
        DateOnly startDate,
        DateOnly? endDate = null
    )
    {
        if (studentId == Guid.Empty)
        {
            throw new ArgumentException("Ученик обязателен.", nameof(studentId));
        }

        if (schoolClassId == Guid.Empty)
        {
            throw new ArgumentException("Класс обязателен.", nameof(schoolClassId));
        }

        if (academicYearId == Guid.Empty)
        {
            throw new ArgumentException("Учебный год обязателен.", nameof(academicYearId));
        }

        if (endDate.HasValue && endDate.Value < startDate)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endDate),
                "Дата окончания не может быть раньше даты начала."
            );
        }

        StudentId = studentId;
        SchoolClassId = schoolClassId;
        AcademicYearId = academicYearId;
        StartDate = startDate;
        EndDate = endDate;
    }

    public void SetEndDate(DateOnly endDate)
    {
        if (endDate < StartDate)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endDate),
                "Дата окончания не может быть раньше даты начала."
            );
        }

        EndDate = endDate;
    }
}
