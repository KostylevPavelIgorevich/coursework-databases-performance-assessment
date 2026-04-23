using BackendCore.BackendCore.API.Endpoints;

namespace BackendCore.BackendCore.API.DependencyInjection;

public static class ApiEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapSchoolApi(this IEndpointRouteBuilder app)
    {
        DataEndpoints.Map(app);
        StudentEndpoints.Map(app);
        ClassEndpoints.Map(app);
        SubjectEndpoints.Map(app);
        TeacherEndpoints.Map(app);
        ScheduleEndpoints.Map(app);
        GradeEndpoints.Map(app);
        return app;
    }
}
