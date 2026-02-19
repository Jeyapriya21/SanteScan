using Microsoft.EntityFrameworkCore;
using santeScan.Server.Data;
using santeScan.Server.Middleware;
using santeScan.Server.Extensions;
using santeScan.Server.Services; // ✅ Assure-toi d'importer le namespace
using santeScan.Server.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Services de base
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=santescan.db"));

// 3. Enregistrement des services (ORDRE IMPORTANT)
// ✅ Enregistre le SessionService AVANT d'appeler ConfigureServices si possible

// Cette méthode doit contenir l'enregistrement de IOcrService et IOllamaService
builder.Services.ConfigureServices(builder.Configuration);

// 4. Gestion des erreurs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// 5. Pipeline de développement
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // ✅ Ajoute ceci pour forcer l'affichage des erreurs détaillées dans ta console Vue
    app.UseDeveloperExceptionPage();
}

app.UseExceptionHandler(); // Utilise ton GlobalExceptionHandler

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// app.ConfigurePipeline(); // Vérifie ce que fait cette méthode, elle peut écraser tes réglages.

app.Run();