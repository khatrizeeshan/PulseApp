using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using PulseApp.Services;

namespace PulseApp.Helpers
{
    public static class EndpointHelper
    {
        public static void MapGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<AttendanceService>().RequireCors("AllowAll");
            endpoints.MapGrpcService<CalendarService>().RequireCors("AllowAll");
            endpoints.MapGrpcService<EmployeeService>().RequireCors("AllowAll");
            endpoints.MapGrpcService<LeaveService>().RequireCors("AllowAll");
        }
    }
}
