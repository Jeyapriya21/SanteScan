using Tesseract;
using santeScan.Server.Services.Interfaces;

namespace santeScan.Server.Services;

public class OcrService : IOcrService
{
    private readonly ILogger<OcrService> _logger;
    private readonly string _tessdataPath;

    public OcrService(ILogger<OcrService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _tessdataPath = configuration["Ocr:TessdataPath"] ?? "./tessdata";
    }

    public string ExtraireTexteAnalyse(string cheminImage)
    {
        if (string.IsNullOrWhiteSpace(cheminImage))
            throw new ArgumentNullException(nameof(cheminImage));

        if (!File.Exists(cheminImage))
            throw new FileNotFoundException("Le fichier image n'existe pas.", cheminImage);

        try
        {
            using var engine = new TesseractEngine(_tessdataPath, "fra", EngineMode.Default);
            using var img = Pix.LoadFromFile(cheminImage);
            using var page = engine.Process(img);
            
            var text = page.GetText();
            _logger.LogInformation("Texte extrait avec succès de {ImagePath}", cheminImage);
            
            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'extraction OCR du fichier {ImagePath}", cheminImage);
            throw;
        }
    }
}
