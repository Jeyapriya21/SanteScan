using Microsoft.EntityFrameworkCore;
using santeScan.Server.Data;
using santeScan.Server.Middleware;
using santeScan.Server.Services;

namespace santeScan.Server.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection ConfigureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Controllers
        services.AddControllers();

        // Exception handling
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // API Documentation
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=santescan.db"));

        // Application Services
        services.AddScoped<IOcrService, OcrService>();
        services.AddScoped<IOllamaService, OllamaService>();

        // HttpClient for OllamaService
        services.AddHttpClient<IOllamaService, OllamaService>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["Ollama:BaseUrl"] ?? "http://localhost:11434");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        return services;
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles();

        // Development-specific middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapControllers();
        app.MapFallbackToFile("/index.html");

        return app;
    }
}