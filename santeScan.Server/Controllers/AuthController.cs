using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using santeScan.Server.Data;
using santeScan.Server.Models;
using santeScan.Server.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace santeScan.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ApplicationDbContext context,
        ISessionService sessionService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _sessionService = sessionService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        [FromHeader(Name = "X-Session-Id")] string? sessionId)
    {
        // Vérifier si l'email existe déjà
        if (await _context.Users.AnyAsync(u => u.Email == request.Email && !u.IsGuest))
            return BadRequest("Cet email est déjà utilisé");

        // Créer l'utilisateur permanent
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            Age = request.Age,
            Gender = request.Gender ?? "Non spécifié",
            IsGuest = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Migrer les analyses guest si sessionId fourni
        int migratedCount = 0;
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var migrated = await _sessionService.MigrateGuestAnalysesToUserAsync(sessionId, user.Id);
            if (migrated)
            {
                migratedCount = await _context.Analyses
                    .Where(a => a.UserId == user.Id)
                    .CountAsync();
            }
        }

        _logger.LogInformation(
            "✅ Utilisateur {UserId} créé. {Count} analyses migrées.",
            user.Id, migratedCount);

        return Ok(new
        {
            userId = user.Id,
            email = user.Email,
            message = "Compte créé avec succès",
            analysesConservees = migratedCount
        });
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}

public record RegisterRequest(
    string Email,
    string Password,
    int Age,
    string? Gender);