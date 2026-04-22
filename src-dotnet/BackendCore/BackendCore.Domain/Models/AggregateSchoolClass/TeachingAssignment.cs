using BackendCore.BackendCore.Domain.Models.Base;

namespace BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;

public class TeachingAssignment : Entity
{
    public int SchoolClassId { get; private set; }
    public int TeacherId { get; private set; }
    public int SubjectId { get; private set; }
    public int AcademicYearId { get; private set; }

    public TeachingAssignment(
        int schoolClassId,
        int teacherId,
        int subjectId,
        int academicYearId
    )
    {
        if (schoolClassId <= 0)
        {
            throw new ArgumentException("Класс обязателен.", nameof(schoolClassId));
        }

        if (teacherId <= 0)
        {
            throw new ArgumentException("Учитель обязателен.", nameof(teacherId));
        }

        if (subjectId <= 0)
        {
            throw new ArgumentException("Предмет обязателен.", nameof(subjectId));
        }

        if (academicYearId <= 0)
        {
            throw new ArgumentException("Учебный год обязателен.", nameof(academicYearId));
        }

        SchoolClassId = schoolClassId;
        TeacherId = teacherId;
        SubjectId = subjectId;
        AcademicYearId = academicYearId;
    }
}
