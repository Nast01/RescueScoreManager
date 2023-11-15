using System.IO;

using Microsoft.EntityFrameworkCore;

using RescueScoreManager.Model;

namespace RescueScoreManager.Data;

public class RescueScoreManagerContext : DbContext
{
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<Club> Clubs => Set<Club>();
    public FileInfo DbPath { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Configure Competition-Organizer One-to-One relation
        //modelBuilder.Entity<Competition>()
        //    .HasOne(c => c.Organizer)
        //    .WithOne(cl => cl.OrganizerCompetition)
        //    .HasForeignKey<Club>(cl => cl.OrganizerCompetitionId)
        //    .IsRequired(true);

        //Configure Competition-Clubs One-to-Many relation
        modelBuilder.Entity<Competition>()
            .HasMany(c => c.Clubs)
            .WithOne(cl => cl.Competition)
            .HasForeignKey(cl => cl.CompetitionId)
            .IsRequired(true);


        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.UseSqlite($"Data Source=C:\\Users\\nast0\\Documents\\RescueScore\\RescueScoreManager\\rsm.ffss");
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
        base.OnConfiguring(optionsBuilder);
    }
}
