using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PulseApp.Data;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace PulseApp.Helpers
{
    public static class DatabaseHelper
    {
        public static async Task CreateDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    if(await context.Database.EnsureCreatedAsync())
                    {
                        await Seeding.RunAsync(context);
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred creating the DB.");
                }
            }
        }

        public static void AddApplicationDbContext(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContextFactory<ApplicationDbContext>(options =>
            {
#if DEBUG
                options.UseSqlServer(config.GetConnectionString("DefaultConnection"))
                    .EnableSensitiveDataLogging();
#else
                options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
#endif
            });

            services.AddScoped(p =>
                p.GetRequiredService<IDbContextFactory<ApplicationDbContext>>()
                .CreateDbContext());
        }
    }
}
