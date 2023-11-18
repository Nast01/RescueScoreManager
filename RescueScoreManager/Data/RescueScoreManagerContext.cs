using System.IO;

using Microsoft.EntityFrameworkCore;

using RescueScoreManager.Model;

namespace RescueScoreManager.Data;

public class RescueScoreManagerContext : DbContext
{
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<Licensee> Licensees => Set<Licensee>();

    public FileInfo DbPath { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Configure Competition-Clubs One-to-Many relation
        modelBuilder.Entity<Competition>()
            .HasMany(c => c.Clubs)
            .WithOne(cl => cl.Competition)
            .HasForeignKey(cl => cl.CompetitionId)
            .IsRequired(true);


        //Configure Referee-RefereeDate One-to-Many relation
        modelBuilder.Entity<RefereeDate>()
            .HasKey(rd => rd.Id);

        modelBuilder.Entity<RefereeDate>()
            .HasOne(rd => rd.Referee)
            .WithMany(r => r.RefereeAvailabilities)
            .HasForeignKey(rd => rd.RefereeId);


        //Configure Club-Licensess One-to-Many relation
        modelBuilder.Entity<Licensee>()
            .HasDiscriminator<string>("LicenseeType")
            .HasValue<Athlete>("Athlete")
            .HasValue<Referee>("Referee");

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.UseSqlite($"Data Source=C:\\Users\\nast0\\Documents\\RescueScore\\RescueScoreManager\\rsm.ffss");
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
        base.OnConfiguring(optionsBuilder);
    }
}
