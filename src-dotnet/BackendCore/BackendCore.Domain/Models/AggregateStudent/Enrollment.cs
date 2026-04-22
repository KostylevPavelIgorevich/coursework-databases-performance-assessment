using BackendCore.BackendCore.Domain.Models.Base;

namespace BackendCore.BackendCore.Domain.Models.AggregateStudent;

public class Enrollment : Entity
{
    public int StudentId { get; private set; }
    public int SchoolClassId { get; private set; }
    public int AcademicYearId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }

    public Enrollment(
        int studentId,
        int schoolClassId,
        int academicYearId,
        DateOnly startDate,
        DateOnly? endDate = null
    )
    {
        if (studentId <= 0)
        {
            throw new ArgumentException("Ученик обязателен.", nameof(studentId));
        }

        if (schoolClassId <= 0)
        {
            throw new ArgumentException("Класс обязателен.", nameof(schoolClassId));
        }

        if (academicYearId <= 0)
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
