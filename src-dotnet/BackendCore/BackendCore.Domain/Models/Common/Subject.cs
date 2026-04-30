using BackendCore.BackendCore.Domain.Models.Base;

namespace BackendCore.BackendCore.Domain.Models.Common;

public class Subject : Entity
{
    public string Name { get; private set; }
    public string? ShortName { get; private set; }

    public Subject(string name, string? shortName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Название предмета обязательно.", nameof(name));
        }

        Name = name.Trim();
        ShortName = string.IsNullOrWhiteSpace(shortName) ? null : shortName.Trim();
    }

    public void Update(string name, string? shortName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Название предмета обязательно.", nameof(name));
        }

        Name = name.Trim();
        ShortName = string.IsNullOrWhiteSpace(shortName) ? null : shortName.Trim();
    }
}
