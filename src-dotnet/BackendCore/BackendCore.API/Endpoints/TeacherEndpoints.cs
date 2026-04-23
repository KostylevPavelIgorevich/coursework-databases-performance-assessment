using BackendCore.BackendCore.API.Contracts;
using BackendCore.BackendCore.Domain.Models.Common;
using BackendCore.BackendCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.API.Endpoints;

public static class TeacherEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/teachers", Create);
        app.MapPut("/api/teachers/{id:int}", Update);
        app.MapDelete("/api/teachers/{id:int}", Delete);
    }

    private static async Task<IResult> Create(
        CreateTeacherRequest request,
        SchoolDbContext db,
        CancellationToken ct
    )
    {
        var teacher = new Teacher(request.LastName, request.FirstName, request.MiddleName);
        await db.Teachers.AddAsync(teacher, ct);
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = teacher.Id });
    }

    private static async Task<IResult> Update(
        int id,
        UpdateTeacherRequest request,
        SchoolDbContext db,
        CancellationToken ct
    )
    {
        var entity = await db.Teachers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return Results.NotFound();
        }

        try
        {
            entity.Update(request.LastName, request.FirstName, request.MiddleName);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }

        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = entity.Id });
    }

    private static async Task<IResult> Delete(int id, SchoolDbContext db, CancellationToken ct)
    {
        var entity = await db.Teachers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return Results.NotFound();
        }
        var teachingAssignments = await db.TeachingAssignments.Where(x => x.TeacherId == id).ToListAsync(ct);
        var assignmentIds = teachingAssignments.Select(x => x.Id).ToList();
        var scheduleSlots = await db.ScheduleSlots
            .Where(x => assignmentIds.Contains(x.TeachingAssignmentId))
            .ToListAsync(ct);
        var slotIds = scheduleSlots.Select(x => x.Id).ToList();
        var lessons = await db.Lessons.Where(x => slotIds.Contains(x.ScheduleSlotId)).ToListAsync(ct);
        var lessonIds = lessons.Select(x => x.Id).ToList();
        var grades = await db.Grades.Where(x => lessonIds.Contains(x.LessonId)).ToListAsync(ct);

        db.Grades.RemoveRange(grades);
        db.Lessons.RemoveRange(lessons);
        db.ScheduleSlots.RemoveRange(scheduleSlots);
        db.TeachingAssignments.RemoveRange(teachingAssignments);
        db.Teachers.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}
