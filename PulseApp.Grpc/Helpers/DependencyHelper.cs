using Microsoft.Extensions.DependencyInjection;
using PulseApp.Services;

namespace PulseApp.Helpers
{
    public static class DependencyHelper
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton<SettingService>();
            services.AddSingleton<EmployeeService>();
            services.AddSingleton<AttendanceService>();
            services.AddSingleton<CalendarService>();
            services.AddSingleton<LeaveService>();
        }
    }
}
