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
    private readonly ILogger<AnalysesController> _logger;
    private readonly IConfiguration _configuration;

    private const int MaxFileSizeInBytes = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };

    public AnalysesController(
        ApplicationDbContext context,
        IOcrService ocrService,
        IOllamaService ollamaService,
        ILogger<AnalysesController> logger,
        IConfiguration configuration)
    {
        _context = context;
        _ocrService = ocrService;
        _ollamaService = ollamaService;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UploadBloodTest(IFormFile file)
    {
        var validationResult = ValidateFile(file);
        if (validationResult != null)
            return validationResult;

        var userId = GetAuthenticatedUserId();
        if (userId == null)
            return Unauthorized("Utilisateur non authentifié.");

        string? filePath = null;
        try
        {
            filePath = await SaveFileTemporarily(file);
            var analysis = await ProcessAnalysis(filePath, userId.Value, file.FileName);
            
            _logger.LogInformation(
                "Analyse {AnalysisId} créée avec succès pour l'utilisateur {UserId}",
                analysis.Id, userId);

            return Ok(new
            {
                analysisId = analysis.Id,
                message = "Analyse terminée avec succès.",
                uploadDate = analysis.UploadDate,
                status = analysis.GlobalStatus
            });
        }
        catch (OcrException ex)
        {
            _logger.LogWarning(ex, "Erreur OCR pour l'utilisateur {UserId}", userId);
            return StatusCode(422, new { error = ex.Message });
        }
        catch (AiAnalysisException ex)
        {
            _logger.LogWarning(ex, "Service IA indisponible pour l'utilisateur {UserId}", userId);
            return StatusCode(503, new { error = ex.Message });
        }
        finally
        {
            CleanupTempFile(filePath);
        }
    }

    [HttpGet("{analysisId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAnalysis(Guid analysisId)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
            return Unauthorized();

        var analysis = await _context.Analyses
            .Include(a => a.Details)
            .FirstOrDefaultAsync(a => a.Id == analysisId && a.UserId == userId);

        if (analysis == null)
            return NotFound();

        return Ok(analysis);
    }

    #region Private Methods

    private IActionResult? ValidateFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Aucun fichier reçu.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return BadRequest($"Format non supporté. Formats acceptés : {string.Join(", ", AllowedExtensions)}");

        if (file.Length > MaxFileSizeInBytes)
            return BadRequest($"Fichier trop volumineux (max {MaxFileSizeInBytes / (1024 * 1024)} MB).");

        return null;
    }

    private Guid? GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Tentative d'accès sans authentification valide");
            return null;
        }
        return userId;
    }

    private static async Task<string> SaveFileTemporarily(IFormFile file)
    {
        var filePath = Path.GetTempFileName();
        await using var stream = File.Create(filePath);
        await file.CopyToAsync(stream);
        return filePath;
    }

    private async Task<BloodTestAnalysis> ProcessAnalysis(
        string filePath,
        Guid userId,
        string fileName)
    {
        _logger.LogInformation(
            "Début du traitement du fichier {FileName} pour l'utilisateur {UserId}",
            fileName, userId);

        var extractedText = ExtractText(filePath, fileName);
        var aiSummary = await AnalyzeText(extractedText, userId);

        var analysis = new BloodTestAnalysis
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UploadDate = DateTime.UtcNow,
            RawText = extractedText,
            AiSummary = aiSummary,
            GlobalStatus = "Analysé"
        };

        _context.Analyses.Add(analysis);
        await _context.SaveChangesAsync();

        return analysis;
    }

    private string ExtractText(string filePath, string fileName)
    {
        try
        {
            var text = _ocrService.ExtraireTexteAnalyse(filePath);
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new OcrException(
                    "Aucun texte extrait. Vérifiez la qualité de l'image.");
            }
            return text;
        }
        catch (Exception ex) when (ex is not OcrException)
        {
            _logger.LogError(ex, "Erreur OCR pour le fichier {FileName}", fileName);
            throw new OcrException(
                "Impossible d'extraire le texte de l'image. Vérifiez la qualité et le format.",
                ex);
        }
    }

    private async Task<string> AnalyzeText(string text, Guid userId)
    {
        try
        {
            return await _ollamaService.AnalyserAnalyseSanguine(text);
        }
        catch (Exception ex) when (ex is not AiAnalysisException)
        {
            _logger.LogError(ex, "Erreur IA pour l'utilisateur {UserId}", userId);
            throw new AiAnalysisException(
                "Le service d'analyse IA est temporairement indisponible.",
                ex);
        }
    }

    private void CleanupTempFile(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return;

        try
        {
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible de supprimer le fichier temporaire {FilePath}", filePath);
        }
    }

    #endregion
}