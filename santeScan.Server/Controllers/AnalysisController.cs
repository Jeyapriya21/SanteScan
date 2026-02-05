using Microsoft.AspNetCore.Mvc;
using santeScan.Server.Data;
using santeScan.Server.Models;
using santeScan.Server.Services;
using santeScan.Server.Exceptions;
using System.Security.Claims;

namespace santeScan.Server.Controllers; 

[ApiController]
[Route("api/[controller]")]
public class AnalysesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly OcrService _ocrService;
    private readonly OllamaService _ollamaService;
    private readonly ILogger<AnalysesController> _logger;

    public AnalysesController(
        ApplicationDbContext context, 
        OcrService ocrService, 
        OllamaService ollamaService,
        ILogger<AnalysesController> logger)
    {
        _context = context;
        _ocrService = ocrService;
        _ollamaService = ollamaService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadBloodTest(IFormFile file)
    {
        // Validation renforcée
        if (file == null || file.Length == 0)
            return BadRequest("Aucun fichier reçu.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
            return BadRequest("Format de fichier non supporté. Formats acceptés : JPG, PNG, PDF.");

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest("Le fichier est trop volumineux (max 10 MB).");

        // Récupération sécurisée du userId
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Tentative d'upload sans authentification valide");
            return Unauthorized("Utilisateur non authentifié.");
        }

        var filePath = Path.GetTempFileName();
        
        try
        {
            _logger.LogInformation("Début du traitement du fichier {FileName} pour l'utilisateur {UserId}", 
                file.FileName, userId);

            // Sauvegarde temporaire
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            // Extraction OCR avec gestion d'erreur spécifique
            string extractedText;
            try
            {
                extractedText = _ocrService.ExtraireTexteAnalyse(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur OCR pour le fichier {FileName}", file.FileName);
                throw new OcrException("Impossible d'extraire le texte de l'image. Vérifiez la qualité et le format.", ex);
            }
            
            if (string.IsNullOrWhiteSpace(extractedText))
            {
                _logger.LogWarning("Aucun texte extrait du fichier {FileName}", file.FileName);
                return BadRequest("Impossible d'extraire du texte de l'image. Veuillez vérifier la qualité de l'image.");
            }

            // Analyse IA avec gestion d'erreur spécifique
            string resumeIA;
            try
            {
                resumeIA = await _ollamaService.AnalyserAnalyseSanguine(extractedText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'analyse IA pour l'utilisateur {UserId}", userId);
                throw new AiAnalysisException("Le service d'analyse IA est temporairement indisponible.", ex);
            }

            // Création de l'analyse
            var analysis = new BloodTestAnalysis
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                UploadDate = DateTime.UtcNow,
                RawText = extractedText,
                AiSummary = resumeIA,
                GlobalStatus = "Analysé"
            };

            _context.Analyses.Add(analysis);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Analyse {AnalysisId} créée avec succès pour l'utilisateur {UserId}", 
                analysis.Id, userId);

            return Ok(new { analysisId = analysis.Id, message = "Analyse terminée avec succès." });
        }
        catch (OcrException)
        {
            return StatusCode(422, "Erreur lors de l'extraction du texte. Veuillez utiliser une image plus claire.");
        }
        catch (AiAnalysisException)
        {
            return StatusCode(503, "Service d'analyse temporairement indisponible. Réessayez dans quelques instants.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur inattendue lors du traitement pour l'utilisateur {UserId}", userId);
            return StatusCode(500, "Une erreur est survenue lors du traitement de l'image.");
        }
        finally
        {
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Impossible de supprimer le fichier temporaire {FilePath}", filePath);
                }
            }
        }
    }
}