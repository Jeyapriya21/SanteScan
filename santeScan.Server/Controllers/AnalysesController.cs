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

        // ✅ Gestion du userId : authentifié OU guest via sessionId
        Guid userId;
        var authenticatedUserId = GetAuthenticatedUserId();

        if (authenticatedUserId.HasValue)
        {
            // Utilisateur authentifié
            userId = authenticatedUserId.Value;
            sessionId = null; // Pas de sessionId pour les utilisateurs authentifiés
        }
        else if (!string.IsNullOrWhiteSpace(sessionId))
        {
            // Utilisateur guest avec sessionId
            userId = await _sessionService.GetOrCreateGuestUserAsync(sessionId);
        }
        else
        {
            return BadRequest("SessionId requis pour les utilisateurs non authentifiés");
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
            _logger.LogWarning(ex, "Erreur OCR");
            return StatusCode(422, new { error = ex.Message });
        }
        catch (AiAnalysisException ex)
        {
            _logger.LogWarning(ex, "Service IA indisponible");
            return StatusCode(503, new { error = ex.Message });
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
            return BadRequest("Aucun fichier reçu.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return BadRequest($"Format non supporté : {string.Join(", ", AllowedExtensions)}");

        if (file.Length > MaxFileSizeInBytes)
            return BadRequest($"Fichier trop volumineux (max 10 MB).");

        return null;
    }

    private Guid? GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private static async Task<string> SaveFileTemporarily(IFormFile file)
    {
        var filePath = Path.GetTempFileName();
        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream);
        return filePath;
    }

    private async Task<BloodTestAnalysis> ProcessAnalysis(
        string filePath,
        Guid userId,
        string? sessionId,
        string fileName)
    {
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

        return analysis;
    }

    private void CleanupTempFile(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            return;

        try
        {
            System.IO.File.Delete(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible de supprimer {FilePath}", filePath);
        }
    }

    #endregion
}