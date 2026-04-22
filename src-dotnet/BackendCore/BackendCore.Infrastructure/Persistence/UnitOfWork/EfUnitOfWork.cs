using BackendCore.BackendCore.Application.Abstractions.Persistence;

namespace BackendCore.BackendCore.Infrastructure.Persistence.UnitOfWork;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly SchoolDbContext _dbContext;

    public EfUnitOfWork(SchoolDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
