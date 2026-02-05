namespace santeScan.Server.Models
{
    public class BloodTestDetail
    {
        public Guid Id { get; set; }
        public Guid AnalysisId { get; set; }

        public string ParameterName { get; set; } // ex: "Cholestérol"
        public double Value { get; set; }         // ex: 1.85
        public string Unit { get; set; }          // ex: "g/L"

        // Pour indiquer si cette ligne précise est hors norme
        public bool IsAbnormal { get; set; }
        public string ReferenceRange { get; set; } // ex: "1.50 - 2.00"
    }
}
