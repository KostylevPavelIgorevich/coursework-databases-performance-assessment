using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.Infrastructure.Persistence.Repositories;

public sealed class ScheduleSlotRepository : IScheduleSlotRepository
{
    private readonly SchoolDbContext _dbContext;

    public ScheduleSlotRepository(SchoolDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ScheduleSlot?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.ScheduleSlots.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.ScheduleSlots.AnyAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsForClassAsync(
        int schoolClassId,
        DayOfWeek dayOfWeek,
        int lessonNumber,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.ScheduleSlots.AnyAsync(
            x => x.SchoolClassId == schoolClassId && x.DayOfWeek == dayOfWeek && x.LessonNumber == lessonNumber,
            cancellationToken
        );
    }

    public async Task AddAsync(ScheduleSlot entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.ScheduleSlots.AddAsync(entity, cancellationToken);
    }
}
