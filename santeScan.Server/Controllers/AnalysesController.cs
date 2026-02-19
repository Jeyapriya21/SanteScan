using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using santeScan.Server.Data;
using santeScan.Server.Models;
using santeScan.Server.Services.Interfaces;
using santeScan.Server.Exceptions;
using System.Security.Claims;

namespace santeScan.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysesController : ControllerBase    
{
    private readonly ApplicationDbContext _context;
    private readonly IOcrService _ocrService;
    private readonly IOllamaService _ollamaService;
    private readonly ISessionService _sessionService;
    private readonly ILogger<AnalysesController> _logger;

    private const int MaxFileSizeInBytes = 10 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };

    public AnalysesController(
        ApplicationDbContext context,
        IOcrService ocrService,
        IOllamaService ollamaService,
        ISessionService sessionService,
        ILogger<AnalysesController> logger)
    {
        _context = context;
        _ocrService = ocrService;
        _ollamaService = ollamaService;
        _sessionService = sessionService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UploadBloodTest(
        IFormFile file,
        [FromHeader(Name = "X-Session-Id")] string? sessionId)
    {
        var validationResult = ValidateFile(file);
        if (validationResult != null)
            return validationResult;

        Guid userId;
        var authenticatedUserId = GetAuthenticatedUserId();

        if (authenticatedUserId.HasValue)
        {
            userId = authenticatedUserId.Value;
            sessionId = null;
        }
        else if (!string.IsNullOrWhiteSpace(sessionId))
        {
            userId = await _sessionService.GetOrCreateGuestUserAsync(sessionId);
        }
        else
        {
            return BadRequest(new { error = "SessionId requis pour les utilisateurs non authentifiés" });
        }

        string? filePath = null;
        try
        {
            filePath = await SaveFileTemporarily(file);
            var analysis = await ProcessAnalysis(filePath, userId, sessionId, file.FileName);

            _logger.LogInformation(
                "Analyse {AnalysisId} créée pour userId={UserId}, sessionId={SessionId}",
                analysis.Id, userId, sessionId ?? "null");

            return Ok(new
            {
                analysisId = analysis.Id,
                message = "Analyse terminée avec succès.",
                uploadDate = analysis.UploadDate,
                status = analysis.GlobalStatus,
                isGuest = !string.IsNullOrWhiteSpace(sessionId)
            });
        }
        catch (OcrException ex)
        {
            _logger.LogError(ex, "Erreur OCR");
            return StatusCode(422, new { error = "Erreur OCR", message = ex.Message, details = ex.InnerException?.Message });
        }
        catch (AiAnalysisException ex)
        {
            _logger.LogError(ex, "Service IA indisponible");
            return StatusCode(503, new { error = "Service IA indisponible", message = ex.Message, details = ex.InnerException?.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur inattendue");
            return StatusCode(500, new { error = "Erreur Interne", message = ex.Message, details = ex.InnerException?.Message });
        }
        finally
        {
            CleanupTempFile(filePath);
        }
    }

    [HttpGet("session/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessionAnalyses(string sessionId)
    {
        var analyses = await _context.Analyses
            .Where(a => a.SessionId == sessionId)
            .OrderByDescending(a => a.UploadDate)
            .Select(a => new
            {
                a.Id,
                a.UploadDate,
                a.GlobalStatus,
                a.AiSummary
            })
            .ToListAsync();

        return Ok(new
        {
            sessionId,
            count = analyses.Count,
            analyses
        });
    }

    [HttpGet("{analysisId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAnalysis(
        Guid analysisId,
        [FromHeader(Name = "X-Session-Id")] string? sessionId)
    {
        var userId = GetAuthenticatedUserId();

        var analysis = await _context.Analyses
            .Include(a => a.Details)
            .FirstOrDefaultAsync(a => a.Id == analysisId);

        if (analysis == null)
            return NotFound();

        // Vérifier les droits d'accès
        if (userId.HasValue && analysis.UserId != userId.Value)
            return Forbid();

        if (!userId.HasValue && analysis.SessionId != sessionId)
            return Forbid();

        return Ok(analysis);
    }

    #region Private Methods

    private IActionResult? ValidateFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Aucun fichier reçu." });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return BadRequest(new { error = $"Format non supporté : {string.Join(", ", AllowedExtensions)}" });

        if (file.Length > MaxFileSizeInBytes)
            return BadRequest(new { error = $"Fichier trop volumineux (max 10 MB)." });

        return null;
    }

    private Guid? GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    // ✅ CORRECTION : Garder l'extension originale du fichier
    private static async Task<string> SaveFileTemporarily(IFormFile file)
    {
        // Créer un nom de fichier temporaire avec l'extension originale
        var tempPath = Path.GetTempPath();
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(tempPath, fileName);

        // Copier le fichier
        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        await file.CopyToAsync(stream);
        await stream.FlushAsync(); // ✅ S'assurer que tout est écrit

        return filePath;
    }

    private async Task<BloodTestAnalysis> ProcessAnalysis(
        string filePath,
        Guid userId,
        string? sessionId,
        string fileName)
    {
        _logger.LogInformation("Début du traitement du fichier {FileName} pour userId={UserId}", fileName, userId);

        // ✅ Vérifier que le fichier existe avant de le traiter
        if (!System.IO.File.Exists(filePath))
        {
            throw new FileNotFoundException($"Le fichier temporaire n'existe pas : {filePath}");
        }

        var extractedText = _ocrService.ExtraireTexteAnalyse(filePath);
        if (string.IsNullOrWhiteSpace(extractedText))
            throw new OcrException("Aucun texte extrait.");

        var aiSummary = await _ollamaService.AnalyserAnalyseSanguine(extractedText);

        var analysis = new BloodTestAnalysis
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SessionId = sessionId,
            UploadDate = DateTime.UtcNow,
            RawText = extractedText,
            AiSummary = aiSummary,
            GlobalStatus = "Analysé"
        };

        _context.Analyses.Add(analysis);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ Analyse {AnalysisId} sauvegardée avec succès", analysis.Id);

        return analysis;
    }

    private void CleanupTempFile(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        try
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                _logger.LogDebug("Fichier temporaire supprimé : {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible de supprimer le fichier temporaire {FilePath}", filePath);
        }
    }

    #endregion
}