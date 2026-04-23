using BackendCore.BackendCore.API.Contracts;
using BackendCore.BackendCore.Application.Abstractions.UseCases;
using BackendCore.BackendCore.Application.Contracts.Common;
using BackendCore.BackendCore.Application.UseCases.Enrollments.EnrollStudent;
using BackendCore.BackendCore.Application.UseCases.Students.CreateStudent;
using BackendCore.BackendCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.API.Endpoints;

public static class StudentEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/students", Create);
        app.MapPut("/api/students/{id:int}", Update);
        app.MapDelete("/api/students/{id:int}", Delete);
    }

    private static async Task<IResult> Create(
        CreateStudentRequest request,
        IUseCase<CreateStudentCommand, OperationResult<int>> createStudent,
        IUseCase<EnrollStudentCommand, OperationResult<int>> enrollStudent,
        SchoolDbContext db,
        CancellationToken ct
    )
    {
        var statusId = request.StudentStatusId;
        var statusExists = await db.StudentStatuses.AnyAsync(x => x.Id == statusId, ct);
        if (!statusExists)
        {
            statusId = await db.StudentStatuses
                .Where(x => x.Name == "Активен")
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
            return Results.BadRequest(new { message = createResult.Error ?? "Не удалось создать ученика." });
        }
        var studentId = createResult.Data;

        var currentYear = await db.AcademicYears
            .OrderByDescending(x => x.IsCurrent)
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
            return Results.BadRequest(new { message = enrollResult.Error ?? "Не удалось зачислить ученика." });
        }

        return Results.Ok(new { id = studentId });
    }

    private static async Task<IResult> Update(
        int id,
        UpdateStudentRequest request,
        SchoolDbContext db,
        CancellationToken ct
    )
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

    private static async Task<IResult> Delete(int id, SchoolDbContext db, CancellationToken ct)
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
}
