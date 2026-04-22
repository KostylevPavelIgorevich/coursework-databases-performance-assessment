namespace BackendCore.BackendCore.Domain.Models.Base;

public abstract class Entity
{
    public Guid Id { get; private set; }

    public Entity()
    {
        Id = Guid.NewGuid();
    }
}
