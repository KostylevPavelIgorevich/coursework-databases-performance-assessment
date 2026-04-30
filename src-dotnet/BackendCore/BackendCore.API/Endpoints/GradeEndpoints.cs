using BackendCore.BackendCore.API.Contracts;
using BackendCore.BackendCore.Domain.Models.AggregateLesson;
using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;
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

        var slotId = await EnsureScheduleSlotAsync(
            db,
            enrollment.SchoolClassId,
            request.SubjectId,
            ct
        );
        if (slotId <= 0)
        {
            return Results.BadRequest(new { message = "Не удалось подготовить расписание для оценки." });
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

        var gradeTypeExists = await db.GradeTypes.AnyAsync(x => x.Id == request.GradeTypeId, ct);
        if (!gradeTypeExists)
        {
            return Results.BadRequest(new { message = "Не найден тип оценки." });
        }

        var grade = new Grade(lesson.Id, request.StudentId, request.GradeTypeId, request.Value);
        await db.Grades.AddAsync(grade, ct);
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = grade.Id });
    }

    private static async Task<int> EnsureScheduleSlotAsync(
        SchoolDbContext db,
        int classId,
        int subjectId,
        CancellationToken ct
    )
    {
        var existingSlotId = await db
            .ScheduleSlots.Join(
                db.TeachingAssignments,
                slot => slot.TeachingAssignmentId,
                ta => ta.Id,
                (slot, ta) => new { slot.Id, slot.SchoolClassId, ta.SubjectId }
            )
            .Where(x => x.SchoolClassId == classId && x.SubjectId == subjectId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(ct);
        if (existingSlotId > 0)
        {
            return existingSlotId;
        }

        var academicYearId = await db
            .SchoolClasses.Where(x => x.Id == classId)
            .Select(x => x.AcademicYearId)
            .FirstOrDefaultAsync(ct);
        if (academicYearId <= 0)
        {
            return 0;
        }

        var teachingAssignment = await db.TeachingAssignments.FirstOrDefaultAsync(
            x =>
                x.SchoolClassId == classId
                && x.SubjectId == subjectId
                && x.AcademicYearId == academicYearId,
            ct
        );

        if (teachingAssignment is null)
        {
            var teacherId = await db.Teachers.Select(x => x.Id).FirstOrDefaultAsync(ct);
            if (teacherId <= 0)
            {
                return 0;
            }

            teachingAssignment = new TeachingAssignment(classId, teacherId, subjectId, academicYearId);
            await db.TeachingAssignments.AddAsync(teachingAssignment, ct);
            await db.SaveChangesAsync(ct);
        }

        var classroomId = await db.Classrooms.Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (classroomId <= 0)
        {
            return 0;
        }

        var occupied = await db
            .ScheduleSlots.Where(x => x.SchoolClassId == classId)
            .Select(x => new { x.DayOfWeek, x.LessonNumber })
            .ToListAsync(ct);

        for (var day = DayOfWeek.Monday; day <= DayOfWeek.Friday; day++)
        {
            for (var lessonNumber = 1; lessonNumber <= 10; lessonNumber++)
            {
                if (occupied.Any(x => x.DayOfWeek == day && x.LessonNumber == lessonNumber))
                {
                    continue;
                }

                var slot = new ScheduleSlot(
                    classId,
                    day,
                    lessonNumber,
                    teachingAssignment.Id,
                    classroomId
                );
                await db.ScheduleSlots.AddAsync(slot, ct);
                await db.SaveChangesAsync(ct);
                return slot.Id;
            }
        }

        return 0;
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
            if (request.GradeTypeId.HasValue)
            {
                var gradeTypeExists = await db.GradeTypes.AnyAsync(
                    x => x.Id == request.GradeTypeId.Value,
                    ct
                );
                if (!gradeTypeExists)
                {
                    return Results.BadRequest(new { message = "Не найден тип оценки." });
                }

                grade.ChangeGradeType(request.GradeTypeId.Value);
            }
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
