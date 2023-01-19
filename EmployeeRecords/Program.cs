using EmployeeRecords.Core.Helpers;
using EmployeeRecords.Core.Repositories;
using EmployeeRecords.Custom.JsonConverters;
using EmployeeRecords.Custom.Middlewares;
using EmployeeRecords.Infrastructure;
using EmployeeRecords.Infrastructure.Data.Repositories;
using EmployeeRecords.Mapping;
using NLog.Web;
using System.Net;

namespace EmployeeRecords;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder.Services, builder.Configuration);

        builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        var app = builder.Build();
        ConfigureApplication(app);

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMvc().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        });

        services.AddAutoMapper(config =>
        {
            config.AddProfile<ApplicationMappingProfile>();
        });

        services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
            options.HttpsPort = int.Parse(configuration["HttpsPort"]);
        });

        services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
        services.AddScoped<IEmployeesRepository, EmployeesRepository>();
        services.AddScoped<IEmployeesFilesRepository, EmployeesFilesRepository>();

        services.AddScoped<IFilesRepository, FilesRepository>();

        services.Configure<FileCriteria>(configuration.GetSection("FileCriteria"));
    }

    private static void ConfigureApplication(IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseHsts();
        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseStatusCodePages();
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action}/{id?}");
        });
    }
}