using System.Text;
using System.Text.Json;
using santeScan.Server.Services.Interfaces;

namespace santeScan.Server.Services;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaService> _logger;
    private readonly string _model;

    public OllamaService(
        HttpClient httpClient, 
        ILogger<OllamaService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _model = configuration["Ollama:Model"] ?? "llama3";
    }

    public async Task<string> AnalyserAnalyseSanguine(string texteBrut)
    {
        if (string.IsNullOrWhiteSpace(texteBrut))
            throw new ArgumentNullException(nameof(texteBrut));

        var prompt = BuildPrompt(texteBrut);
        var requestBody = new
        {
            model = _model,
            prompt = prompt,
            stream = false
        };

        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody), 
                Encoding.UTF8, 
                "application/json");

            var response = await _httpClient.PostAsync("/api/generate", content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            
            var result = doc.RootElement.GetProperty("response").GetString();
            _logger.LogInformation("Analyse IA complétée avec succès");
            
            return result ?? string.Empty;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erreur de communication avec le service Ollama");
            throw new InvalidOperationException(
                "Le service d'analyse IA est indisponible", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erreur de parsing de la réponse Ollama");
            throw new InvalidOperationException(
                "Réponse invalide du service d'analyse", ex);
        }
    }

    private static string BuildPrompt(string texteBrut)
    {
        return $@"Tu es un assistant médical. Voici un texte issu d'une prise de sang : '{texteBrut}'.
Extrais les valeurs importantes et fais un résumé très court et simple pour le patient.
Dis si le bilan semble bon ou s'il y a des anomalies.
Termine obligatoirement par : 'Merci de consulter votre médecin pour interpréter ces résultats.'";
    }
}
