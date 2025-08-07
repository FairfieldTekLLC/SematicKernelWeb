using Microsoft.AspNetCore.Authentication.Negotiate;
using SematicKernelWeb.Classes;
using SematicKernelWeb.Hubs;
using SematicKernelWeb.Logging;
using SematicKernelWeb.Middleware;
using SematicKernelWeb.SemanticKernel;
using SematicKernelWeb.SemanticKernel.Database;
using SematicKernelWeb.Services;

namespace SematicKernelWeb;

public class Program
{
    public static void Main(string[] args)
    {
        Config.Instance.Load();
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSignalR();
        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddSingleton<IBackendWorker, BackendWorker>();
        builder.Services.AddSingleton<ILoggerProvider, LoggerProvider>();
        builder.AddDatabaseServices();
        builder.AddSemanticKernel();
        builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
        builder.Services.AddAuthorization(options => { options.FallbackPolicy = options.DefaultPolicy; });
        builder.Services.AddMvc().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        builder.Services
            .AddControllersWithViews(options =>
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
            .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

        builder.Services.AddSession();
        builder.Services.AddMemoryCache();


        WebApplication app = builder.Build();
        app.ConfigureDatabaseServices();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthorization();
        app.UseMiddleware<AddIdentityMiddleware>();

        app.MapStaticAssets();
        app.MapControllerRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();
        app.MapHub<ChatHub>("/chatHub");
        app.Run();
    }
}