using BackendCore.BackendCore.Application.Abstractions.Persistence;
using BackendCore.BackendCore.Infrastructure.Persistence;
using BackendCore.BackendCore.Infrastructure.Persistence.Repositories;
using BackendCore.BackendCore.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BackendCore.BackendCore.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString
    )
    {
        services.AddDbContext<SchoolDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IStudentStatusRepository, StudentStatusRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<ISchoolClassRepository, SchoolClassRepository>();
        services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
        services.AddScoped<ITeachingAssignmentRepository, TeachingAssignmentRepository>();
        services.AddScoped<IClassroomRepository, ClassroomRepository>();
        services.AddScoped<IScheduleSlotRepository, ScheduleSlotRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<IGradeTypeRepository, GradeTypeRepository>();
        services.AddScoped<IGradeRepository, GradeRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
