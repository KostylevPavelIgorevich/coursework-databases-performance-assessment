using BackendCore.BackendCore.Domain.Models.Base;

namespace BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;

public class Classroom : Entity
{
    public string Number { get; private set; }
    public string? Building { get; private set; }

    public Classroom(string number, string? building = null)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new ArgumentException("Номер кабинета обязателен.", nameof(number));
        }

        Number = number.Trim().ToUpperInvariant();
        Building = string.IsNullOrWhiteSpace(building) ? null : building.Trim();
    }
}
