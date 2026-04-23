using BackendCore.BackendCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.API.Endpoints;

public static class DataEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/data", GetData);
        app.MapPost("/api/admin/reseed-default", ReseedDefault);
    }

    private static async Task<IResult> GetData(SchoolDbContext db, CancellationToken ct)
    {
        var classes = await db.SchoolClasses
            .OrderBy(x => x.Grade)
            .ThenBy(x => x.Letter)
            .Select(x => new { x.Id, Title = x.Grade + x.Letter })
            .ToListAsync(ct);

        var subjects = await db.Subjects
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync(ct);

        var teachers = await db.Teachers
            .OrderBy(x => x.LastName)
            .Select(x => new { x.Id, FullName = x.LastName + " " + x.FirstName + " " + x.MiddleName })
            .ToListAsync(ct);

        var students = await db.Students
            .OrderBy(x => x.LastName)
            .Select(x => new
            {
                x.Id,
                x.LastName,
                x.FirstName,
                x.MiddleName,
                ClassId = db.Enrollments
                    .Where(e => e.StudentId == x.Id && e.EndDate == null)
                    .OrderByDescending(e => e.StartDate)
                    .Select(e => e.SchoolClassId)
                    .FirstOrDefault(),
            })
            .ToListAsync(ct);

        var schedule = await db.ScheduleSlots
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.LessonNumber)
            .Select(x => new
            {
                x.Id,
                x.SchoolClassId,
                Day = x.DayOfWeek.ToString(),
                x.LessonNumber,
                SubjectId = db.TeachingAssignments
                    .Where(t => t.Id == x.TeachingAssignmentId)
                    .Select(t => t.SubjectId)
                    .FirstOrDefault(),
                TeacherId = db.TeachingAssignments
                    .Where(t => t.Id == x.TeachingAssignmentId)
                    .Select(t => t.TeacherId)
                    .FirstOrDefault(),
            })
            .ToListAsync(ct);

        var grades = await db.Grades
            .OrderByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.StudentId,
                SubjectId = db.Lessons
                    .Where(l => l.Id == x.LessonId)
                    .Join(db.ScheduleSlots, l => l.ScheduleSlotId, s => s.Id, (_, s) => s.TeachingAssignmentId)
                    .Join(db.TeachingAssignments, ta => ta, t => t.Id, (_, t) => t.SubjectId)
                    .FirstOrDefault(),
                Date = db.Lessons.Where(l => l.Id == x.LessonId).Select(l => l.Date).FirstOrDefault(),
                x.Value,
            })
            .ToListAsync(ct);

        return Results.Ok(new { classes, subjects, teachers, students, schedule, grades });
    }

    private static async Task<IResult> ReseedDefault(SchoolDbContext db, CancellationToken ct)
    {
        await SchoolDbInitializer.ResetToDefaultAsync(db, ct);
        return Results.Ok(new { message = "Базовые данные восстановлены." });
    }
}
