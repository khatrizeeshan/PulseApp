using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PulseApp.Protos;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.ClientFactory;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

namespace PulseApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services
                  .AddBlazorise(options =>
                  {
                      options.ChangeTextOnKeyPress = true;
                  })
                  .AddBootstrapProviders() 
                  .AddFontAwesomeIcons();

            builder.RootComponents.Add<App>("#app");

            //builder.Services.AddSingleton(services =>
            //{
            //    // Get the service address from appsettings.json
            //    var config = services.GetRequiredService<IConfiguration>();
            //    var gRPCbackendUrl = config["GrpcBackendUrl"];

            //    // Create a channel with a GrpcWebHandler that is addressed to the backend server.
            //    //
            //    // GrpcWebText is used because server streaming requires it. If server streaming is not used in your app
            //    // then GrpcWeb is recommended because it produces smaller messages.
            //    var httpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());

            //    return GrpcChannel.ForAddress(gRPCbackendUrl, new GrpcChannelOptions { HttpHandler = httpHandler });
            //});

            Action<IServiceProvider, GrpcClientFactoryOptions> client = 
                (services, options) => {
                        options.Address = new Uri("https://localhost:5001");
                };

            Func<GrpcWebHandler> handler = () => new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler());

            builder.Services.AddGrpcClient<LeaveManager.LeaveManagerClient>(client)
                .ConfigurePrimaryHttpMessageHandler(handler);

            builder.Services.AddGrpcClient<AttendanceManager.AttendanceManagerClient>(client)
                .ConfigurePrimaryHttpMessageHandler(handler);

            builder.Services.AddGrpcClient<CalendarManager.CalendarManagerClient>(client)
                .ConfigurePrimaryHttpMessageHandler(handler);

            builder.Services.AddGrpcClient<EmployeeManager.EmployeeManagerClient>(client)
                .ConfigurePrimaryHttpMessageHandler(handler);

            //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            var host = builder.Build();

            host.Services
              .UseBootstrapProviders()
              .UseFontAwesomeIcons();

            await host.RunAsync();
        }
    }
}
