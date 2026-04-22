using BackendCore.BackendCore.Domain.Models.Base;

namespace BackendCore.BackendCore.Domain.Models.Common;

public class Period : Entity
{
    public Guid AcademicYearId { get; private set; }
    public string Name { get; private set; }
    public int Order { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }

    public Period(Guid academicYearId, string name, int order, DateOnly startDate, DateOnly endDate)
    {
        if (academicYearId == Guid.Empty)
        {
            throw new ArgumentException("Учебный год обязателен.", nameof(academicYearId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Название периода обязательно.", nameof(name));
        }

        if (order < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(order),
                "Порядковый номер периода должен быть положительным."
            );
        }

        if (endDate <= startDate)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endDate),
                "Дата окончания должна быть позже даты начала."
            );
        }

        AcademicYearId = academicYearId;
        Name = name.Trim();
        Order = order;
        StartDate = startDate;
        EndDate = endDate;
    }
}
