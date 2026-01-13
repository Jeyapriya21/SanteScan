namespace santeScan.Server.Models
{
    public class BloodTestAnalysis
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime UploadDate { get; set; }

        // Le texte brut extrait par Tesseract (pour archivage)
        public string RawText { get; set; }

        // Le résumé simple généré par l'IA
        public string AiSummary { get; set; }

        // Statut : "Normal", "Attention", "Critique"
        public string GlobalStatus { get; set; }

        // Message de sécurité obligatoire
        public string MedicalDisclaimer => "Ce résumé est informatif et ne remplace pas un avis médical.";

        // Relation : Une analyse contient plusieurs lignes de résultats
        public List<BloodTestDetail> Details { get; set; } = new List<BloodTestDetail>();
    }
}
