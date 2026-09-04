using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeKeeper.DataAccess.Entities;
using TimeKeeper.Domain.Services;
using TimeKeeper.Domain.Services.Interfaces;

namespace TimeKeeper.App.Api.DIServices.Extensiions
{
    public static class DIServiceExtensions
    {

        /// <summary>
        /// Adds services which interact with the domain to the application
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTimeKeeperDomainServices(this IServiceCollection services) 
        {
            services.AddScoped<ProjectServiceOptions>();
            services.AddTransient<IProjectService, ProjectService>();

            services.AddScoped<LaborerServiceOptions>();
            services.AddTransient<ILaborerService, LaborerService>();

            services.AddScoped<WorkItemServiceOptions>();
            services.AddTransient<IWorkItemService, WorkItemService>();    

            services.AddScoped<TimeEntryServiceOptions>();
            services.AddTransient<ITimeEntryService, TimeEntryService>();
            return services;
        }

        /// <summary>
        /// Configures the TimeKeeperDb and adds it to the application.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection AddTimeKeeperDbServices(this IServiceCollection services, ConfigurationManager? config)
        {
            string? connectionString = config?.GetConnectionString("TimeKeeperDB");

            if (connectionString != null)
            {
                services.AddDbContext<TimeKeeperDbContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                });
            }

            return services;
        }

    }
}
