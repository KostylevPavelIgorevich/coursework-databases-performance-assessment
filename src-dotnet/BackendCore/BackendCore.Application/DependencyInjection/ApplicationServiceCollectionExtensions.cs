using BackendCore.BackendCore.Application.Abstractions.UseCases;
using BackendCore.BackendCore.Application.Contracts.Common;
using BackendCore.BackendCore.Application.UseCases.Enrollments.EnrollStudent;
using BackendCore.BackendCore.Application.UseCases.Grades.AssignGrade;
using BackendCore.BackendCore.Application.UseCases.Lessons.CreateLesson;
using BackendCore.BackendCore.Application.UseCases.ScheduleSlots.CreateScheduleSlot;
using BackendCore.BackendCore.Application.UseCases.Students.CreateStudent;
using Microsoft.Extensions.DependencyInjection;

namespace BackendCore.BackendCore.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<
            IUseCase<CreateStudentCommand, OperationResult<int>>,
            CreateStudentHandler
        >();
        services.AddScoped<
            IUseCase<EnrollStudentCommand, OperationResult<int>>,
            EnrollStudentHandler
        >();
        services.AddScoped<
            IUseCase<CreateScheduleSlotCommand, OperationResult<int>>,
            CreateScheduleSlotHandler
        >();
        services.AddScoped<
            IUseCase<CreateLessonCommand, OperationResult<int>>,
            CreateLessonHandler
        >();
        services.AddScoped<
            IUseCase<AssignGradeCommand, OperationResult<int>>,
            AssignGradeHandler
        >();

        return services;
    }
}
