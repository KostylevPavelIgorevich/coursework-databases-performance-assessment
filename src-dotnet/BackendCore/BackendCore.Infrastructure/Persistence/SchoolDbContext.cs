using BackendCore.BackendCore.Domain.Models.AggregateLesson;
using BackendCore.BackendCore.Domain.Models.AggregateSchoolClass;
using BackendCore.BackendCore.Domain.Models.AggregateStudent;
using BackendCore.BackendCore.Domain.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.BackendCore.Infrastructure.Persistence;

public class SchoolDbContext : DbContext
{
    public SchoolDbContext(DbContextOptions<SchoolDbContext> options)
        : base(options) { }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<StudentStatus> StudentStatuses => Set<StudentStatus>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<SchoolClass> SchoolClasses => Set<SchoolClass>();
    public DbSet<TeachingAssignment> TeachingAssignments => Set<TeachingAssignment>();
    public DbSet<ScheduleSlot> ScheduleSlots => Set<ScheduleSlot>();
    public DbSet<Classroom> Classrooms => Set<Classroom>();

    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<GradeType> GradeTypes => Set<GradeType>();

    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Period> Periods => Set<Period>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchoolDbContext).Assembly);
    }
}
