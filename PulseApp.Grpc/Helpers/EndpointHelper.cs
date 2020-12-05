using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using PulseApp.Services;

namespace PulseApp.Helpers
{
    public static class EndpointHelper
    {
        public static void Map(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<LeaveService>();
        }
    }
}
