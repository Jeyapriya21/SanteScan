using System.ComponentModel.DataAnnotations;

namespace santeScan.Server.Models;

/// <summary>
/// Représente un paramètre individuel d'une analyse de sang
/// (ex: Hémoglobine, Glycémie, Cholestérol, etc.)
/// </summary>
public class BloodTestDetail
{
    /// <summary>
    /// Identifiant unique du détail
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// ID de l'analyse parente
    /// </summary>
    [Required]
    public Guid AnalysisId { get; set; }
    
    /// <summary>
    /// Nom du paramètre (ex: "Hémoglobine", "Glycémie")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ParameterName { get; set; } = string.Empty;
    
    /// <summary>
    /// Valeur mesurée (peut être null si non détecté)
    /// </summary>
    public double? Value { get; set; }
    
    /// <summary>
    /// Unité de mesure (ex: "g/L", "mmol/L")
    /// </summary>
    [MaxLength(50)]
    public string? Unit { get; set; }
    
    /// <summary>
    /// Plage de référence normale (ex: "4.5-5.5 g/L")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ReferenceRange { get; set; } = string.Empty; // ✅ CORRECTION
    
    /// <summary>
    /// Statut du résultat : "Normal", "Bas", "Élevé", "Critique"
    /// </summary>
    [MaxLength(50)]
    public string Status { get; set; } = "Normal";
    
    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
