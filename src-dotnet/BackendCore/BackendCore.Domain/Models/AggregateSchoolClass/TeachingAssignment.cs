using BackendCore.BackendCore.Domain.Models.Base;

namespace BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;

public class TeachingAssignment : Entity
{
    public Guid SchoolClassId { get; private set; }
    public Guid TeacherId { get; private set; }
    public Guid SubjectId { get; private set; }
    public Guid AcademicYearId { get; private set; }

    public TeachingAssignment(
        Guid schoolClassId,
        Guid teacherId,
        Guid subjectId,
        Guid academicYearId
    )
    {
        if (schoolClassId == Guid.Empty)
        {
            throw new ArgumentException("Класс обязателен.", nameof(schoolClassId));
        }

        if (teacherId == Guid.Empty)
        {
            throw new ArgumentException("Учитель обязателен.", nameof(teacherId));
        }

        if (subjectId == Guid.Empty)
        {
            throw new ArgumentException("Предмет обязателен.", nameof(subjectId));
        }

        if (academicYearId == Guid.Empty)
        {
            throw new ArgumentException("Учебный год обязателен.", nameof(academicYearId));
        }

        SchoolClassId = schoolClassId;
        TeacherId = teacherId;
        SubjectId = subjectId;
        AcademicYearId = academicYearId;
    }
}
