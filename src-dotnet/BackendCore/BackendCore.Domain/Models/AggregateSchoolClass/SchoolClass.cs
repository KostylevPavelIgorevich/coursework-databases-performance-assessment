using BackendCore.BackendCore.Domain.Models.Base;

namespace BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;

public class SchoolClass : Entity
{
    public int Grade { get; private set; }
    public string Letter { get; private set; }
    public Guid AcademicYearId { get; private set; }

    public SchoolClass(int grade, string letter, Guid academicYearId)
    {
        if (grade is < 1 or > 11)
        {
            throw new ArgumentOutOfRangeException(
                nameof(grade),
                "Номер класса должен быть в диапазоне 1..11."
            );
        }

        if (string.IsNullOrWhiteSpace(letter))
        {
            throw new ArgumentException("Литера класса обязательна.", nameof(letter));
        }

        if (academicYearId == Guid.Empty)
        {
            throw new ArgumentException("Учебный год обязателен.", nameof(academicYearId));
        }

        Grade = grade;
        Letter = letter.Trim().ToUpperInvariant();
        AcademicYearId = academicYearId;
    }
}
