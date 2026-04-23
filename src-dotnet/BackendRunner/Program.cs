using BackendCore.BackendCore.Application.Abstractions.UseCases;
using BackendCore.BackendCore.Application.Contracts.Common;
using BackendCore.BackendCore.Application.DependencyInjection;
using BackendCore.BackendCore.Application.UseCases.Enrollments.EnrollStudent;
using BackendCore.BackendCore.Application.UseCases.Students.CreateStudent;
using BackendCore.BackendCore.Infrastructure.DependencyInjection;
using BackendCore.BackendCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("SchoolDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Строка подключения 'ConnectionStrings:SchoolDb' не задана."
    );
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "frontend",
        policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
    );
});

var app = builder.Build();
app.UseCors("frontend");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();
    await SchoolDbInitializer.InitializeAsync(dbContext);
}

app.MapGet(
    "/api/data",
    async (SchoolDbContext db, CancellationToken ct) =>
    {
        var classes = await db
            .SchoolClasses.OrderBy(x => x.Grade)
            .ThenBy(x => x.Letter)
            .Select(x => new { x.Id, Title = x.Grade + x.Letter })
            .ToListAsync(ct);

        var subjects = await db
            .Subjects.OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync(ct);

        var teachers = await db
            .Teachers.OrderBy(x => x.LastName)
            .Select(x => new
            {
                x.Id,
                FullName = x.LastName + " " + x.FirstName + " " + x.MiddleName,
            })
            .ToListAsync(ct);

        var students = await db
            .Students.OrderBy(x => x.LastName)
            .Select(x => new
            {
                x.Id,
                x.LastName,
                x.FirstName,
                x.MiddleName,
                ClassId = db
                    .Enrollments.Where(e => e.StudentId == x.Id && e.EndDate == null)
                    .OrderByDescending(e => e.StartDate)
                    .Select(e => e.SchoolClassId)
                    .FirstOrDefault(),
            })
            .ToListAsync(ct);

        var schedule = await db
            .ScheduleSlots.OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.LessonNumber)
            .Select(x => new
            {
                x.Id,
                x.SchoolClassId,
                Day = x.DayOfWeek.ToString(),
                x.LessonNumber,
                SubjectId = db
                    .TeachingAssignments.Where(t => t.Id == x.TeachingAssignmentId)
                    .Select(t => t.SubjectId)
                    .FirstOrDefault(),
                TeacherId = db
                    .TeachingAssignments.Where(t => t.Id == x.TeachingAssignmentId)
                    .Select(t => t.TeacherId)
                    .FirstOrDefault(),
            })
            .ToListAsync(ct);

        var grades = await db
            .Grades.OrderByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.StudentId,
                SubjectId = db
                    .Lessons.Where(l => l.Id == x.LessonId)
                    .Join(
                        db.ScheduleSlots,
                        l => l.ScheduleSlotId,
                        s => s.Id,
                        (_, s) => s.TeachingAssignmentId
                    )
                    .Join(db.TeachingAssignments, ta => ta, t => t.Id, (_, t) => t.SubjectId)
                    .FirstOrDefault(),
                Date = db
                    .Lessons.Where(l => l.Id == x.LessonId)
                    .Select(l => l.Date)
                    .FirstOrDefault(),
                x.Value,
            })
            .ToListAsync(ct);

        return Results.Ok(
            new
            {
                classes,
                subjects,
                teachers,
                students,
                schedule,
                grades,
            }
        );
    }
);

app.MapPost(
    "/api/admin/reseed-default",
    async (SchoolDbContext db, CancellationToken ct) =>
    {
        await SchoolDbInitializer.ResetToDefaultAsync(db, ct);
        return Results.Ok(new { message = "Базовые данные восстановлены." });
    }
);

app.MapPost(
    "/api/students",
    async (
        CreateStudentRequest request,
        IUseCase<CreateStudentCommand, OperationResult<int>> createStudent,
        IUseCase<EnrollStudentCommand, OperationResult<int>> enrollStudent,
        SchoolDbContext db,
        CancellationToken ct
    ) =>
    {
        var statusId = request.StudentStatusId;
        var statusExists = await db.StudentStatuses.AnyAsync(x => x.Id == statusId, ct);
        if (!statusExists)
        {
            statusId = await db
                .StudentStatuses.Where(x => x.Name == "Активен")
                .Select(x => x.Id)
                .FirstOrDefaultAsync(ct);
            if (statusId <= 0)
            {
                statusId = await db.StudentStatuses.Select(x => x.Id).FirstOrDefaultAsync(ct);
            }
        }
        if (statusId <= 0)
        {
            return Results.BadRequest(new { message = "Не найден статус ученика." });
        }

        var createResult = await createStudent.ExecuteAsync(
            new CreateStudentCommand(
                request.LastName,
                request.FirstName,
                request.MiddleName,
                request.BirthDate,
                statusId
            ),
            ct
        );
        if (!createResult.IsSuccess)
        {
            return Results.BadRequest(
                new { message = createResult.Error ?? "Не удалось создать ученика." }
            );
        }
        var studentId = createResult.Data;

        var currentYear = await db
            .AcademicYears.OrderByDescending(x => x.IsCurrent)
            .ThenByDescending(x => x.StartDate)
            .FirstOrDefaultAsync(ct);

        if (currentYear is null)
        {
            return Results.BadRequest(new { message = "Не найден учебный год для зачисления." });
        }

        var enrollResult = await enrollStudent.ExecuteAsync(
            new EnrollStudentCommand(
                studentId,
                request.ClassId,
                currentYear.Id,
                DateOnly.FromDateTime(DateTime.UtcNow)
            ),
            ct
        );
        if (!enrollResult.IsSuccess)
        {
            return Results.BadRequest(
                new { message = enrollResult.Error ?? "Не удалось зачислить ученика." }
            );
        }

        return Results.Ok(new { id = studentId });
    }
);

app.MapDelete(
    "/api/students/{id:int}",
    async (int id, SchoolDbContext db, CancellationToken ct) =>
    {
        var student = await db.Students.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (student is null)
        {
            return Results.NotFound();
        }

        var enrollments = await db.Enrollments.Where(x => x.StudentId == id).ToListAsync(ct);
        var grades = await db.Grades.Where(x => x.StudentId == id).ToListAsync(ct);

        db.Enrollments.RemoveRange(enrollments);
        db.Grades.RemoveRange(grades);
        db.Students.Remove(student);
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
);

app.MapPut(
    "/api/classes/{id:int}",
    async (int id, UpdateClassRequest request, SchoolDbContext db, CancellationToken ct) =>
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
);

app.MapPut(
    "/api/students/{id:int}",
    async (int id, UpdateStudentRequest request, SchoolDbContext db, CancellationToken ct) =>
    {
        var student = await db.Students.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (student is null)
        {
            return Results.NotFound();
        }

        try
        {
            student.UpdateProfile(
                request.LastName,
                request.FirstName,
                request.MiddleName,
                request.BirthDate
            );
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }

        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = student.Id });
    }
);

app.MapPost(
    "/api/schedule",
    async (CreateScheduleRequest request, SchoolDbContext db, CancellationToken ct) =>
    {
        if (!Enum.TryParse<DayOfWeek>(request.Day, true, out var dayOfWeek))
        {
            return Results.BadRequest(new { message = "Некорректный день недели." });
        }

        var currentYear = await db
            .AcademicYears.OrderByDescending(x => x.IsCurrent)
            .ThenByDescending(x => x.StartDate)
            .FirstOrDefaultAsync(ct);
        if (currentYear is null)
        {
            return Results.BadRequest(new { message = "Не найден учебный год." });
        }

        var teachingAssignment = await db.TeachingAssignments.FirstOrDefaultAsync(
            x =>
                x.SchoolClassId == request.ClassId
                && x.SubjectId == request.SubjectId
                && x.TeacherId == request.TeacherId
                && x.AcademicYearId == currentYear.Id,
            ct
        );
        if (teachingAssignment is null)
        {
            teachingAssignment =
                new BackendCore.BackendCore.Domain.Models.AggregateSchoolClass.TeachingAssignment(
                    request.ClassId,
                    request.TeacherId,
                    request.SubjectId,
                    currentYear.Id
                );
            await db.TeachingAssignments.AddAsync(teachingAssignment, ct);
            await db.SaveChangesAsync(ct);
        }

        var classroomId = await db.Classrooms.Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (classroomId <= 0)
        {
            return Results.BadRequest(
                new { message = "Не найден кабинет для назначения в расписание." }
            );
        }

        var slot = new BackendCore.BackendCore.Domain.Models.AggregateSchoolClass.ScheduleSlot(
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
);

app.MapDelete(
    "/api/schedule/{id:int}",
    async (int id, SchoolDbContext db, CancellationToken ct) =>
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
);

app.MapPut(
    "/api/subjects/{id:int}",
    async (int id, UpdateSubjectRequest request, SchoolDbContext db, CancellationToken ct) =>
    {
        var entity = await db.Subjects.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return Results.NotFound();
        }

        try
        {
            entity.Update(request.Name, request.ShortName);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }

        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = entity.Id });
    }
);

app.MapPut(
    "/api/grades/{id:int}",
    async (int id, UpdateGradeRequest request, SchoolDbContext db, CancellationToken ct) =>
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
);

app.MapPost(
    "/api/grades",
    async (CreateGradeRequest request, SchoolDbContext db, CancellationToken ct) =>
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
            lesson = new BackendCore.BackendCore.Domain.Models.AggregateLesson.Lesson(
                slotId,
                request.Date,
                "Урок"
            );
            await db.Lessons.AddAsync(lesson, ct);
            await db.SaveChangesAsync(ct);
        }

        var gradeTypeId = await db.GradeTypes.Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (gradeTypeId <= 0)
        {
            return Results.BadRequest(new { message = "Не найден тип оценки." });
        }

        var grade = new BackendCore.BackendCore.Domain.Models.AggregateLesson.Grade(
            lesson.Id,
            request.StudentId,
            gradeTypeId,
            request.Value
        );
        await db.Grades.AddAsync(grade, ct);
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = grade.Id });
    }
);

app.MapDelete(
    "/api/grades/{id:int}",
    async (int id, SchoolDbContext db, CancellationToken ct) =>
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
);

app.MapPut(
    "/api/teachers/{id:int}",
    async (int id, UpdateTeacherRequest request, SchoolDbContext db, CancellationToken ct) =>
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
);

app.MapPost(
    "/api/classes",
    async (CreateClassRequest request, SchoolDbContext db, CancellationToken ct) =>
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

        var schoolClass =
            new BackendCore.BackendCore.Domain.Models.AggregateSchoolClass.SchoolClass(
                grade,
                letter,
                currentYear.Id
            );
        await db.SchoolClasses.AddAsync(schoolClass, ct);
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = schoolClass.Id });
    }
);

app.MapDelete(
    "/api/classes/{id:int}",
    async (int id, SchoolDbContext db, CancellationToken ct) =>
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
);

app.MapPut(
    "/api/schedule/{id:int}",
    async (int id, UpdateScheduleRequest request, SchoolDbContext db, CancellationToken ct) =>
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

        var currentYearId = await db
            .SchoolClasses.Where(x => x.Id == request.ClassId)
            .Select(x => x.AcademicYearId)
            .FirstOrDefaultAsync(ct);
        if (currentYearId <= 0)
        {
            return Results.BadRequest(new { message = "Не найден учебный год для класса." });
        }

        var teachingAssignment = await db.TeachingAssignments.FirstOrDefaultAsync(
            x =>
                x.SchoolClassId == request.ClassId
                && x.SubjectId == request.SubjectId
                && x.TeacherId == request.TeacherId
                && x.AcademicYearId == currentYearId,
            ct
        );
        if (teachingAssignment is null)
        {
            teachingAssignment =
                new BackendCore.BackendCore.Domain.Models.AggregateSchoolClass.TeachingAssignment(
                    request.ClassId,
                    request.TeacherId,
                    request.SubjectId,
                    currentYearId
                );
            await db.TeachingAssignments.AddAsync(teachingAssignment, ct);
            await db.SaveChangesAsync(ct);
        }

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
);

app.MapPost(
    "/api/subjects",
    async (CreateSubjectRequest request, SchoolDbContext db, CancellationToken ct) =>
    {
        var subject = new BackendCore.BackendCore.Domain.Models.Common.Subject(
            request.Name,
            request.ShortName
        );
        await db.Subjects.AddAsync(subject, ct);
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = subject.Id });
    }
);

app.MapDelete(
    "/api/subjects/{id:int}",
    async (int id, SchoolDbContext db, CancellationToken ct) =>
    {
        var entity = await db.Subjects.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return Results.NotFound();
        }
        var teachingAssignments = await db
            .TeachingAssignments.Where(x => x.SubjectId == id)
            .ToListAsync(ct);
        var assignmentIds = teachingAssignments.Select(x => x.Id).ToList();
        var scheduleSlots = await db
            .ScheduleSlots.Where(x => assignmentIds.Contains(x.TeachingAssignmentId))
            .ToListAsync(ct);
        var slotIds = scheduleSlots.Select(x => x.Id).ToList();
        var lessons = await db
            .Lessons.Where(x => slotIds.Contains(x.ScheduleSlotId))
            .ToListAsync(ct);
        var lessonIds = lessons.Select(x => x.Id).ToList();
        var grades = await db.Grades.Where(x => lessonIds.Contains(x.LessonId)).ToListAsync(ct);

        db.Grades.RemoveRange(grades);
        db.Lessons.RemoveRange(lessons);
        db.ScheduleSlots.RemoveRange(scheduleSlots);
        db.TeachingAssignments.RemoveRange(teachingAssignments);
        db.Subjects.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }
);

app.MapPost(
    "/api/teachers",
    async (CreateTeacherRequest request, SchoolDbContext db, CancellationToken ct) =>
    {
        var teacher = new BackendCore.BackendCore.Domain.Models.Common.Teacher(
            request.LastName,
            request.FirstName,
            request.MiddleName
        );
        await db.Teachers.AddAsync(teacher, ct);
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = teacher.Id });
    }
);

app.MapDelete(
    "/api/teachers/{id:int}",
    async (int id, SchoolDbContext db, CancellationToken ct) =>
    {
        var entity = await db.Teachers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return Results.NotFound();
        }
        var teachingAssignments = await db
            .TeachingAssignments.Where(x => x.TeacherId == id)
            .ToListAsync(ct);
        var assignmentIds = teachingAssignments.Select(x => x.Id).ToList();
        var scheduleSlots = await db
            .ScheduleSlots.Where(x => assignmentIds.Contains(x.TeachingAssignmentId))
            .ToListAsync(ct);
        var slotIds = scheduleSlots.Select(x => x.Id).ToList();
        var lessons = await db
            .Lessons.Where(x => slotIds.Contains(x.ScheduleSlotId))
            .ToListAsync(ct);
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
);

app.Run("http://localhost:5050");

public sealed record CreateStudentRequest(
    string LastName,
    string FirstName,
    string MiddleName,
    DateOnly BirthDate,
    int StudentStatusId,
    int ClassId
);

public sealed record CreateClassRequest(string Title);

public sealed record UpdateClassRequest(string Title);

public sealed record CreateSubjectRequest(string Name, string? ShortName = null);

public sealed record UpdateSubjectRequest(string Name, string? ShortName = null);

public sealed record CreateTeacherRequest(string LastName, string FirstName, string MiddleName);

public sealed record UpdateTeacherRequest(string LastName, string FirstName, string MiddleName);

public sealed record CreateScheduleRequest(
    int ClassId,
    string Day,
    int LessonNumber,
    int SubjectId,
    int TeacherId
);

public sealed record UpdateScheduleRequest(
    int ClassId,
    string Day,
    int LessonNumber,
    int SubjectId,
    int TeacherId
);

public sealed record CreateGradeRequest(int StudentId, int SubjectId, DateOnly Date, int Value);

public sealed record UpdateStudentRequest(
    string LastName,
    string FirstName,
    string MiddleName,
    DateOnly BirthDate
);

public sealed record UpdateGradeRequest(int Value, string? Comment = null);
