using System.ComponentModel.DataAnnotations;

namespace santeScan.Server.Models;

public class User
{
    public Guid Id { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Range(1, 150)]
    public int Age { get; set; }
    
    [MaxLength(20)]
    public string Gender { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
