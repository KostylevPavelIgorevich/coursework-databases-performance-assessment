using BackendCore.BackendCore.API.Contracts;
using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;
using BackendCore.BackendCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.API.Endpoints;

public static class ScheduleEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/schedule", Create);
        app.MapPut("/api/schedule/{id:int}", Update);
        app.MapDelete("/api/schedule/{id:int}", Delete);
    }

    private static async Task<IResult> Create(
        CreateScheduleRequest request,
        SchoolDbContext db,
        CancellationToken ct
    )
    {
        if (!Enum.TryParse<DayOfWeek>(request.Day, true, out var dayOfWeek))
        {
            return Results.BadRequest(new { message = "Некорректный день недели." });
        }

        var currentYear = await db.AcademicYears
            .OrderByDescending(x => x.IsCurrent)
            .ThenByDescending(x => x.StartDate)
            .FirstOrDefaultAsync(ct);
        if (currentYear is null)
        {
            return Results.BadRequest(new { message = "Не найден учебный год." });
        }

        var teachingAssignment = await EnsureTeachingAssignmentAsync(
            db,
            request.ClassId,
            request.SubjectId,
            request.TeacherId,
            currentYear.Id,
            ct
        );

        var classroomId = await db.Classrooms.Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (classroomId <= 0)
        {
            return Results.BadRequest(new { message = "Не найден кабинет для назначения в расписание." });
        }

        var slot = new ScheduleSlot(
            request.ClassId,
            dayOfWeek,
            request.LessonNumber,
            teachingAssignment.Id,
            classroomId
        );
        await db.ScheduleSlots.AddAsync(slot, ct);
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { id = slot.Id });
    }

    private static async Task<IResult> Update(
        int id,
        UpdateScheduleRequest request,
        SchoolDbContext db,
        CancellationToken ct
    )
    {
        var slot = await db.ScheduleSlots.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (slot is null)
        {
            return Results.NotFound();
        }

        if (!Enum.TryParse<DayOfWeek>(request.Day, true, out var dayOfWeek))
        {
            return Results.BadRequest(new { message = "Некорректный день недели." });
        }

        var currentYearId = await db.SchoolClasses
            .Where(x => x.Id == request.ClassId)
            .Select(x => x.AcademicYearId)
            .FirstOrDefaultAsync(ct);
        if (currentYearId <= 0)
        {
            return Results.BadRequest(new { message = "Не найден учебный год для класса." });
        }

        var teachingAssignment = await EnsureTeachingAssignmentAsync(
            db,
            request.ClassId,
            request.SubjectId,
            request.TeacherId,
            currentYearId,
            ct
        );

        var classroomId = await db.Classrooms.Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (classroomId <= 0)
        {
            return Results.BadRequest(new { message = "Не найден кабинет для расписания." });
        }

        try
        {
            slot.Update(dayOfWeek, request.LessonNumber, teachingAssignment.Id, classroomId);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }

        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = slot.Id });
    }

    private static async Task<IResult> Delete(int id, SchoolDbContext db, CancellationToken ct)
    {
        var slot = await db.ScheduleSlots.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (slot is null)
        {
            return Results.NotFound();
        }

        var lessons = await db.Lessons.Where(x => x.ScheduleSlotId == id).ToListAsync(ct);
        var lessonIds = lessons.Select(x => x.Id).ToList();
        var grades = await db.Grades.Where(x => lessonIds.Contains(x.LessonId)).ToListAsync(ct);

        db.Grades.RemoveRange(grades);
        db.Lessons.RemoveRange(lessons);
        db.ScheduleSlots.Remove(slot);
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private static async Task<TeachingAssignment> EnsureTeachingAssignmentAsync(
        SchoolDbContext db,
        int classId,
        int subjectId,
        int teacherId,
        int academicYearId,
        CancellationToken ct
    )
    {
        var teachingAssignment = await db.TeachingAssignments.FirstOrDefaultAsync(
            x =>
                x.SchoolClassId == classId
                && x.SubjectId == subjectId
                && x.TeacherId == teacherId
                && x.AcademicYearId == academicYearId,
            ct
        );

        if (teachingAssignment is not null)
        {
            return teachingAssignment;
        }

        teachingAssignment = new TeachingAssignment(classId, teacherId, subjectId, academicYearId);
        await db.TeachingAssignments.AddAsync(teachingAssignment, ct);
        await db.SaveChangesAsync(ct);
        return teachingAssignment;
    }
}
