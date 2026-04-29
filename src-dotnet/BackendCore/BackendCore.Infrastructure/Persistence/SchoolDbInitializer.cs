using BackendCore.BackendCore.Domain.Models.AggregateLesson;
using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;
using BackendCore.BackendCore.Domain.Models.AggregateStudent;
using BackendCore.BackendCore.Domain.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.Infrastructure.Persistence;

public static class SchoolDbInitializer
{
    public static async Task InitializeAsync(
        SchoolDbContext dbContext,
        CancellationToken cancellationToken = default
    )
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await dbContext.Students.AnyAsync(cancellationToken))
        {
            return;
        }

        await SeedDefaultDataAsync(dbContext, cancellationToken);
    }

    public static async Task ResetToDefaultAsync(
        SchoolDbContext dbContext,
        CancellationToken cancellationToken = default
    )
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        dbContext.Grades.RemoveRange(dbContext.Grades);
        dbContext.Lessons.RemoveRange(dbContext.Lessons);
        dbContext.ScheduleSlots.RemoveRange(dbContext.ScheduleSlots);
        dbContext.TeachingAssignments.RemoveRange(dbContext.TeachingAssignments);
        dbContext.Enrollments.RemoveRange(dbContext.Enrollments);
        dbContext.Students.RemoveRange(dbContext.Students);
        dbContext.SchoolClasses.RemoveRange(dbContext.SchoolClasses);
        dbContext.GradeTypes.RemoveRange(dbContext.GradeTypes);
        dbContext.StudentStatuses.RemoveRange(dbContext.StudentStatuses);
        dbContext.Periods.RemoveRange(dbContext.Periods);
        dbContext.Teachers.RemoveRange(dbContext.Teachers);
        dbContext.Subjects.RemoveRange(dbContext.Subjects);
        dbContext.Classrooms.RemoveRange(dbContext.Classrooms);
        dbContext.AcademicYears.RemoveRange(dbContext.AcademicYears);
        await dbContext.SaveChangesAsync(cancellationToken);

        await SeedDefaultDataAsync(dbContext, cancellationToken);
    }

    private static async Task SeedDefaultDataAsync(
        SchoolDbContext dbContext,
        CancellationToken cancellationToken
    )
    {

        var year2023 = new AcademicYear(new DateOnly(2023, 9, 1), new DateOnly(2024, 5, 31), false);
        var year2024 = new AcademicYear(new DateOnly(2024, 9, 1), new DateOnly(2025, 5, 31), false);
        var year2025 = new AcademicYear(new DateOnly(2025, 9, 1), new DateOnly(2026, 5, 31), true);
        dbContext.AcademicYears.AddRange(year2023, year2024, year2025);
        await dbContext.SaveChangesAsync(cancellationToken);

        var period1 = new Period(
            year2025.Id,
            "1 четверть",
            1,
            new DateOnly(2025, 9, 1),
            new DateOnly(2025, 10, 31)
        );
        var period2 = new Period(
            year2025.Id,
            "2 четверть",
            2,
            new DateOnly(2025, 11, 1),
            new DateOnly(2025, 12, 28)
        );
        var period3 = new Period(
            year2025.Id,
            "3 четверть",
            3,
            new DateOnly(2026, 1, 10),
            new DateOnly(2026, 3, 20)
        );
        dbContext.Periods.AddRange(period1, period2, period3);

        var activeStatus = new StudentStatus("Активен", "Ученик обучается в школе");
        var transferredStatus = new StudentStatus("Переведен", "Ученик переведен в другой класс");
        var archivedStatus = new StudentStatus("Архив", "Ученик выбыл");
        dbContext.StudentStatuses.AddRange(activeStatus, transferredStatus, archivedStatus);

        var teacher1 = new Teacher("Иванова", "Мария", "Петровна");
        var teacher2 = new Teacher("Сидоров", "Алексей", "Викторович");
        var teacher3 = new Teacher("Кузнецова", "Елена", "Сергеевна");
        dbContext.Teachers.AddRange(teacher1, teacher2, teacher3);

        var subject1 = new Subject("Математика", "Матем.");
        var subject2 = new Subject("Русский язык", "Рус. яз.");
        var subject3 = new Subject("История", "Ист.");
        dbContext.Subjects.AddRange(subject1, subject2, subject3);

        var classroom1 = new Classroom("101", "Корпус А");
        var classroom2 = new Classroom("203", "Корпус А");
        var classroom3 = new Classroom("305", "Корпус Б");
        dbContext.Classrooms.AddRange(classroom1, classroom2, classroom3);

        await dbContext.SaveChangesAsync(cancellationToken);

        var class8a = new SchoolClass(8, "А", year2025.Id);
        var class9a = new SchoolClass(9, "А", year2025.Id);
        var class10b = new SchoolClass(10, "Б", year2025.Id);
        dbContext.SchoolClasses.AddRange(class8a, class9a, class10b);
        await dbContext.SaveChangesAsync(cancellationToken);

        var student1 = new Student(
            "Мартынюк",
            "Максим",
            "Владимирович",
            new DateOnly(2011, 3, 14),
            activeStatus.Id
        );
        var student2 = new Student(
            "Петрова",
            "Алина",
            "Игоревна",
            new DateOnly(2010, 7, 2),
            activeStatus.Id
        );
        var student3 = new Student(
            "Николаев",
            "Денис",
            "Андреевич",
            new DateOnly(2009, 11, 21),
            activeStatus.Id
        );
        dbContext.Students.AddRange(student1, student2, student3);
        await dbContext.SaveChangesAsync(cancellationToken);

        var enrollment1 = new Enrollment(
            student1.Id,
            class8a.Id,
            year2025.Id,
            new DateOnly(2025, 9, 1)
        );
        var enrollment2 = new Enrollment(
            student2.Id,
            class9a.Id,
            year2025.Id,
            new DateOnly(2025, 9, 1)
        );
        var enrollment3 = new Enrollment(
            student3.Id,
            class10b.Id,
            year2025.Id,
            new DateOnly(2025, 9, 1)
        );
        dbContext.Enrollments.AddRange(enrollment1, enrollment2, enrollment3);

        var assignment1 = new TeachingAssignment(class8a.Id, teacher1.Id, subject1.Id, year2025.Id);
        var assignment2 = new TeachingAssignment(class9a.Id, teacher2.Id, subject2.Id, year2025.Id);
        var assignment3 = new TeachingAssignment(
            class10b.Id,
            teacher3.Id,
            subject3.Id,
            year2025.Id
        );
        dbContext.TeachingAssignments.AddRange(assignment1, assignment2, assignment3);

        var gradeType1 = new GradeType("Ответ у доски", "Устный ответ");
        var gradeType2 = new GradeType("Контрольная", "Письменная работа");
        var gradeType3 = new GradeType("Самостоятельная", "Краткая проверочная");
        dbContext.GradeTypes.AddRange(gradeType1, gradeType2, gradeType3);

        await dbContext.SaveChangesAsync(cancellationToken);

        var slot1 = new ScheduleSlot(
            class8a.Id,
            DayOfWeek.Monday,
            1,
            assignment1.Id,
            classroom1.Id
        );
        var slot2 = new ScheduleSlot(
            class9a.Id,
            DayOfWeek.Tuesday,
            2,
            assignment2.Id,
            classroom2.Id
        );
        var slot3 = new ScheduleSlot(
            class10b.Id,
            DayOfWeek.Wednesday,
            3,
            assignment3.Id,
            classroom3.Id
        );
        dbContext.ScheduleSlots.AddRange(slot1, slot2, slot3);
        await dbContext.SaveChangesAsync(cancellationToken);

        var lesson1 = new Lesson(slot1.Id, new DateOnly(2026, 2, 2), "Линейные уравнения");
        var lesson2 = new Lesson(
            slot2.Id,
            new DateOnly(2026, 2, 3),
            "Сложноподчиненные предложения"
        );
        var lesson3 = new Lesson(slot3.Id, new DateOnly(2026, 2, 4), "Реформы Петра I");
        dbContext.Lessons.AddRange(lesson1, lesson2, lesson3);
        await dbContext.SaveChangesAsync(cancellationToken);

        var grade1 = new Grade(
            lesson1.Id,
            student1.Id,
            gradeType1.Id,
            5,
            "Отличная работа у доски"
        );
        var grade2 = new Grade(lesson2.Id, student2.Id, gradeType2.Id, 4, "Небольшие ошибки");
        var grade3 = new Grade(lesson3.Id, student3.Id, gradeType3.Id, 5, "Уверенное знание темы");
        dbContext.Grades.AddRange(grade1, grade2, grade3);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
