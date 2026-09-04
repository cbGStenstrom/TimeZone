using Radzen;
using TimeKeeper.App.Api.DIServices.Extensiions;
using TimeKeeper.App.Api.Services;

namespace TimeKeeper.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddScoped<ThemeService>();

            builder.Services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssemblyContaining<Domain.Models.Laborer>();
                config.RegisterServicesFromAssemblyContaining<Program>();
            });

            builder.Services.AddScoped<SessionService>();
            builder.Services.AddScoped<BrowserStorageService>();
            builder.Services
                .AddTimeKeeperDomainServices()
                .AddTimeKeeperDbServices(builder.Configuration);

            builder.Services.AddRadzenComponents();
            builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(
                options =>
                {
                    options.DetailedErrors = true;
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<Components.App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
