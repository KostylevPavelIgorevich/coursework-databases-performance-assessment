using BackendCore.BackendCore.Domain.Models.Base;

namespace BackendCore.BackendCore.Domain.Models.AggregateStudent;

public class Student : Entity
{
    public string LastName { get; private set; }
    public string FirstName { get; private set; }
    public string MiddleName { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public int StudentStatusId { get; private set; }

    public Student(
        string lastName,
        string firstName,
        string middleName,
        DateOnly birthDate,
        int studentStatusId
    )
    {
        LastName = ValidateRequired(lastName, nameof(lastName));
        FirstName = ValidateRequired(firstName, nameof(firstName));
        MiddleName = ValidateRequired(middleName, nameof(middleName));

        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentOutOfRangeException(
                nameof(birthDate),
                "Дата рождения не может быть в будущем."
            );
        }

        if (studentStatusId <= 0)
        {
            throw new ArgumentException("Статус ученика обязателен.", nameof(studentStatusId));
        }

        BirthDate = birthDate;
        StudentStatusId = studentStatusId;
    }

    public void ChangeStatus(int studentStatusId)
    {
        if (studentStatusId <= 0)
        {
            throw new ArgumentException("Статус ученика обязателен.", nameof(studentStatusId));
        }

        StudentStatusId = studentStatusId;
    }

    public void UpdateProfile(string lastName, string firstName, string middleName, DateOnly birthDate)
    {
        LastName = ValidateRequired(lastName, nameof(lastName));
        FirstName = ValidateRequired(firstName, nameof(firstName));
        MiddleName = ValidateRequired(middleName, nameof(middleName));

        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentOutOfRangeException(
                nameof(birthDate),
                "Дата рождения не может быть в будущем."
            );
        }

        BirthDate = birthDate;
    }

    private static string ValidateRequired(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Поле обязательно для заполнения.", paramName);
        }

        return value.Trim();
    }
}
