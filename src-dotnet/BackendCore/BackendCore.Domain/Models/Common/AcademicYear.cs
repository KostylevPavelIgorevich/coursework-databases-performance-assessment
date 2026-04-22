using BackendCore.BackendCore.Domain.Models.Base;

namespace BackendCore.BackendCore.Domain.Models.Common;

public class AcademicYear : Entity
{
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public bool IsCurrent { get; private set; }

    public AcademicYear(DateOnly startDate, DateOnly endDate, bool isCurrent = false)
    {
        if (endDate <= startDate)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endDate),
                "Дата окончания должна быть позже даты начала."
            );
        }

        StartDate = startDate;
        EndDate = endDate;
        IsCurrent = isCurrent;
    }

    public void SetCurrent(bool isCurrent)
    {
        IsCurrent = isCurrent;
    }
}
