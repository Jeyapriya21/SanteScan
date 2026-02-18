using Microsoft.EntityFrameworkCore;
using santeScan.Server.Data;
using santeScan.Server.Middleware;
using santeScan.Server.Services;
using santeScan.Server.Services.Interfaces;

namespace santeScan.Server.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection ConfigureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ✅ AJOUTER CORS pour permettre au frontend de communiquer
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", builder =>
            {
                builder
                    .WithOrigins(
                        "http://localhost:5173",   // Vite dev server
                        "https://localhost:5173",
                        "http://localhost:53703",  // SPA proxy
                        "https://localhost:53703"
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        // Application Services avec interfaces
        services.AddScoped<IOcrService, OcrService>();
        services.AddScoped<IOllamaService, OllamaService>();
        services.AddScoped<ISessionService, SessionService>();

        // HttpClient pour OllamaService
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
        // ✅ CORS doit être AVANT les autres middlewares
        app.UseCors("AllowFrontend");

        // ✅ COMMENTER ces lignes si vous n'avez pas de fichiers statiques
        // app.UseDefaultFiles();
        // app.UseStaticFiles();

        // Development-specific middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapControllers();
        
        // ✅ COMMENTER si pas de SPA intégré
        // app.MapFallbackToFile("/index.html");

        return app;
    }
}