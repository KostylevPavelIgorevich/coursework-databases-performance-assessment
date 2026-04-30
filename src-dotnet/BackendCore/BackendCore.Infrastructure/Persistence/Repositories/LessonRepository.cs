using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Domain.Models.AggregateLesson;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Repositories;

public sealed class LessonRepository : ILessonRepository
{
    private readonly SchoolDbContext _dbContext;

    public LessonRepository(SchoolDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Lesson?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Lessons.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Lessons.AnyAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsBySlotAndDateAsync(
        int scheduleSlotId,
        DateOnly date,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.Lessons.AnyAsync(
            x => x.ScheduleSlotId == scheduleSlotId && x.Date == date,
            cancellationToken
        );
    }

    public async Task AddAsync(Lesson entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Lessons.AddAsync(entity, cancellationToken);
    }
}
