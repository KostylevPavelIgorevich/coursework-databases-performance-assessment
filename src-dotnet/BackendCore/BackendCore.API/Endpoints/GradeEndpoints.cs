using BackendCore.BackendCore.API.Contracts;
using BackendCore.BackendCore.Domain.Models.AggregateLesson;
using BackendCore.BackendCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.API.Endpoints;

public static class GradeEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/grades", Create);
        app.MapPut("/api/grades/{id:int}", Update);
        app.MapDelete("/api/grades/{id:int}", Delete);
    }

    private static async Task<IResult> Create(
        CreateGradeRequest request,
        SchoolDbContext db,
        CancellationToken ct
    )
    {
        var enrollment = await db
            .Enrollments.Where(x => x.StudentId == request.StudentId)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefaultAsync(ct);
        if (enrollment is null)
        {
            return Results.BadRequest(
                new { message = "Не найден класс ученика для выставления оценки." }
            );
        }

        var slotId = await db
            .ScheduleSlots.Join(
                db.TeachingAssignments,
                slot => slot.TeachingAssignmentId,
                ta => ta.Id,
                (slot, ta) =>
                    new
                    {
                        slot.Id,
                        slot.SchoolClassId,
                        ta.SubjectId,
                    }
            )
            .Where(x =>
                x.SchoolClassId == enrollment.SchoolClassId && x.SubjectId == request.SubjectId
            )
            .Select(x => x.Id)
            .FirstOrDefaultAsync(ct);
        if (slotId <= 0)
        {
            return Results.BadRequest(
                new { message = "Не найден слот расписания для выбранного предмета." }
            );
        }

        var lesson = await db.Lessons.FirstOrDefaultAsync(
            x => x.ScheduleSlotId == slotId && x.Date == request.Date,
            ct
        );
        if (lesson is null)
        {
            lesson = new Lesson(slotId, request.Date, "Урок");
            await db.Lessons.AddAsync(lesson, ct);
            await db.SaveChangesAsync(ct);
        }

        var gradeTypeId = await db.GradeTypes.Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (gradeTypeId <= 0)
        {
            return Results.BadRequest(new { message = "Не найден тип оценки." });
        }

        var grade = new Grade(lesson.Id, request.StudentId, gradeTypeId, request.Value);
        await db.Grades.AddAsync(grade, ct);
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = grade.Id });
    }

    private static async Task<IResult> Update(
        int id,
        UpdateGradeRequest request,
        SchoolDbContext db,
        CancellationToken ct
    )
    {
        var grade = await db.Grades.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (grade is null)
        {
            return Results.NotFound();
        }

        try
        {
            grade.ChangeValue(request.Value);
            grade.ChangeComment(request.Comment);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }

        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = grade.Id });
    }

    private static async Task<IResult> Delete(int id, SchoolDbContext db, CancellationToken ct)
    {
        var grade = await db.Grades.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (grade is null)
        {
            return Results.NotFound();
        }

        db.Grades.Remove(grade);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}
