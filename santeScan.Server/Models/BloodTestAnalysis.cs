using System.ComponentModel.DataAnnotations;

namespace santeScan.Server.Models;

public class BloodTestAnalysis
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    
    [Required]
    public string RawText { get; set; } = string.Empty;
    
    public string? AiSummary { get; set; }
    
    [MaxLength(50)]
    public string GlobalStatus { get; set; } = "En attente";
    
    public string MedicalDisclaimer => 
        "Ce résumé est informatif et ne remplace pas un avis médical.";
    
    public List<BloodTestDetail> Details { get; set; } = new();
}
