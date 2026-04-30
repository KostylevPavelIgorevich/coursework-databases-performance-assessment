using BackendCore.BackendCore.API.Contracts;
using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;
using BackendCore.BackendCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.API.Endpoints;

public static class ClassEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/classes", Create);
        app.MapPut("/api/classes/{id:int}", Update);
        app.MapDelete("/api/classes/{id:int}", Delete);
    }

    private static async Task<IResult> Create(
        CreateClassRequest request,
        SchoolDbContext db,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < 2)
        {
            return Results.BadRequest(
                new { message = "Название класса должно быть в формате 8А." }
            );
        }

        var trimmed = request.Title.Trim().ToUpperInvariant();
        var letter = trimmed[^1].ToString();
        if (!int.TryParse(trimmed[..^1], out var grade))
        {
            return Results.BadRequest(
                new { message = "Название класса должно быть в формате 8А." }
            );
        }

        var currentYear = await db
            .AcademicYears.OrderByDescending(x => x.IsCurrent)
            .ThenByDescending(x => x.StartDate)
            .FirstOrDefaultAsync(ct);
        if (currentYear is null)
        {
            return Results.BadRequest(new { message = "Не найден учебный год." });
        }

        var exists = await db.SchoolClasses.AnyAsync(
            x => x.Grade == grade && x.Letter == letter && x.AcademicYearId == currentYear.Id,
            ct
        );
        if (exists)
        {
            return Results.BadRequest(new { message = "Такой класс уже существует." });
        }

        var schoolClass = new SchoolClass(grade, letter, currentYear.Id);
        await db.SchoolClasses.AddAsync(schoolClass, ct);
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = schoolClass.Id });
    }

    private static async Task<IResult> Update(
        int id,
        UpdateClassRequest request,
        SchoolDbContext db,
        CancellationToken ct
    )
    {
        var entity = await db.SchoolClasses.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < 2)
        {
            return Results.BadRequest(
                new { message = "Название класса должно быть в формате 8А." }
            );
        }

        var trimmed = request.Title.Trim().ToUpperInvariant();
        var letter = trimmed[^1].ToString();
        if (!int.TryParse(trimmed[..^1], out var grade))
        {
            return Results.BadRequest(
                new { message = "Название класса должно быть в формате 8А." }
            );
        }

        try
        {
            entity.Update(grade, letter);
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
        var entity = await db.SchoolClasses.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return Results.NotFound();
        }
        var teachingAssignments = await db
            .TeachingAssignments.Where(x => x.SchoolClassId == id)
            .ToListAsync(ct);
        var assignmentIds = teachingAssignments.Select(x => x.Id).ToList();
        var scheduleSlots = await db
            .ScheduleSlots.Where(x =>
                x.SchoolClassId == id || assignmentIds.Contains(x.TeachingAssignmentId)
            )
            .ToListAsync(ct);
        var slotIds = scheduleSlots.Select(x => x.Id).ToList();
        var lessons = await db
            .Lessons.Where(x => slotIds.Contains(x.ScheduleSlotId))
            .ToListAsync(ct);
        var lessonIds = lessons.Select(x => x.Id).ToList();
        var grades = await db.Grades.Where(x => lessonIds.Contains(x.LessonId)).ToListAsync(ct);
        var enrollments = await db.Enrollments.Where(x => x.SchoolClassId == id).ToListAsync(ct);

        db.Grades.RemoveRange(grades);
        db.Lessons.RemoveRange(lessons);
        db.ScheduleSlots.RemoveRange(scheduleSlots);
        db.TeachingAssignments.RemoveRange(teachingAssignments);
        db.Enrollments.RemoveRange(enrollments);
        db.SchoolClasses.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}
