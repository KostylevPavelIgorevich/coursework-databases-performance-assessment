using BackendCore.BackendCore.Domain.Models.Base;

namespace BackendCore.BackendCore.Domain.Models.Common;

public class Teacher : Entity
{
    public string LastName { get; private set; }
    public string FirstName { get; private set; }
    public string MiddleName { get; private set; }

    public Teacher(string lastName, string firstName, string middleName)
    {
        LastName = ValidateRequired(lastName, nameof(lastName));
        FirstName = ValidateRequired(firstName, nameof(firstName));
        MiddleName = ValidateRequired(middleName, nameof(middleName));
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
