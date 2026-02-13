using Microsoft.EntityFrameworkCore;
using santeScan.Server.Data;
using santeScan.Server.Middleware;
using santeScan.Server.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

    // Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=santescan.db"));

// Application Services (HttpClient + interfaces)
builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

// Configure pipeline
app.ConfigurePipeline();

app.Run();
