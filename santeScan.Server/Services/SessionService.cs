using Microsoft.EntityFrameworkCore;
using santeScan.Server.Data;
using santeScan.Server.Models;
using santeScan.Server.Services.Interfaces;

namespace santeScan.Server.Services;

public class SessionService : ISessionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SessionService> _logger;

    public SessionService(ApplicationDbContext context, ILogger<SessionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> GetOrCreateGuestUserAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("SessionId ne peut pas être vide", nameof(sessionId));

        // Chercher un utilisateur guest existant avec ce sessionId
        var guestUser = await _context.Users
            .FirstOrDefaultAsync(u => u.SessionId == sessionId && u.IsGuest);

        if (guestUser != null)
        {
            _logger.LogInformation("Utilisateur guest trouvé : {UserId} pour session {SessionId}", 
                guestUser.Id, sessionId);
            return guestUser.Id;
        }

        // Créer un nouvel utilisateur guest
        guestUser = new User
        {
            Id = Guid.NewGuid(),
            Email = $"guest_{sessionId}@santescan.local",
            PasswordHash = "guest-no-password",
            IsGuest = true,
            SessionId = sessionId,
            Age = 0,
            Gender = "Non spécifié",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(guestUser);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ Nouvel utilisateur guest créé : {UserId} pour session {SessionId}", 
            guestUser.Id, sessionId);

        return guestUser.Id;
    }

    public async Task<bool> MigrateGuestAnalysesToUserAsync(string sessionId, Guid permanentUserId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return false;

        // Trouver toutes les analyses liées à cette session
        var guestAnalyses = await _context.Analyses
            .Where(a => a.SessionId == sessionId)
            .ToListAsync();

        if (!guestAnalyses.Any())
        {
            _logger.LogInformation("Aucune analyse à migrer pour la session {SessionId}", sessionId);
            return false;
        }

        // Migrer les analyses vers l'utilisateur permanent
        foreach (var analysis in guestAnalyses)
        {
            analysis.UserId = permanentUserId;
            analysis.SessionId = null; // Nettoyer le sessionId
        }

        // Trouver et supprimer l'utilisateur guest
        var guestUser = await _context.Users
            .FirstOrDefaultAsync(u => u.SessionId == sessionId && u.IsGuest);

        if (guestUser != null)
        {
            _context.Users.Remove(guestUser);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ {Count} analyses migrées de la session {SessionId} vers l'utilisateur {UserId}", 
            guestAnalyses.Count, sessionId, permanentUserId);

        return true;
    }

    public async Task<int> GetGuestAnalysisCountAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return 0;

        return await _context.Analyses
            .Where(a => a.SessionId == sessionId)
            .CountAsync();
    }
}