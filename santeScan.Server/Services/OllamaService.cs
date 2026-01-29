namespace santeScan.Server.Services
{
    using System.Text;
    using System.Text.Json;

    public class OllamaService
    {
        private readonly HttpClient _httpClient;

        public OllamaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> AnalyserAnalyseSanguine(string texteBrut)
        {
            // Le prompt magique qui évite de coder les noms d'analyses à la main
            var prompt = $"Tu es un assistant médical. Voici un texte issu d'une prise de sang : '{texteBrut}'. " +
                         "Extrais les valeurs importantes et fais un résumé très court et simple pour le patient. " +
                         "Dis si le bilan semble bon ou s'il y a des anomalies. " +
                         "Termine obligatoirement par : 'Merci de consulter votre médecin pour interpréter ces résultats.'";

            var requestBody = new { model = "llama3", prompt = prompt, stream = false };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://localhost:11434/api/generate", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            // Extraction de la réponse texte
            using var doc = JsonDocument.Parse(jsonResponse);
            return doc.RootElement.GetProperty("response").GetString();
        }
    }
}
