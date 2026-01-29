using Microsoft.EntityFrameworkCore;
using santeScan.Server.Models;

namespace santeScan.Server.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Vos tables en base de données
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

        // Sécurité : Indexer l'email pour des recherches rapides
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}