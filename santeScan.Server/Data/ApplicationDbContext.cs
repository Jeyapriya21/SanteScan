using Microsoft.EntityFrameworkCore;
using santeScan.Server.Models;

namespace santeScan.Server.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<BloodTestAnalysis> Analyses { get; set; }
    public DbSet<BloodTestDetail> AnalysisDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relation 1-à-plusieurs : Une analyse a plusieurs détails
        modelBuilder.Entity<BloodTestAnalysis>()
            .HasMany(a => a.Details)
            .WithOne()
            .HasForeignKey(d => d.AnalysisId);

        // ✅ Index sur l'email pour recherches rapides
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        // ✅ NOUVEAU : Index sur SessionId pour recherches guests
        modelBuilder.Entity<User>()
            .HasIndex(u => u.SessionId);
        
        modelBuilder.Entity<BloodTestAnalysis>()
            .HasIndex(a => a.SessionId);
    }
}